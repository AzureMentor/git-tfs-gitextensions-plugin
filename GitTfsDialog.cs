﻿using GitUIPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GitTfs.GitExtensions.Plugin
{
    public partial class GitTfsDialog : Form
    {
        private readonly IGitUICommands _commands;
        private readonly SettingsContainer _settings;

        public GitTfsDialog(IGitUICommands commands, SettingsContainer settings, IEnumerable<string> tfsRemotes)
        {
            _commands = commands;
            _settings = settings;

            InitializeComponent();
            TfsRemoteComboBox.DataSource = tfsRemotes.ToList();
            InitializeFromSettings();

            this.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
        }

        private void InitializeFromSettings()
        {
            InitializeTfsRemotes();
            InitializePull();
            InitializePush();
        }

        private void InitializeTfsRemotes()
        {
            TfsRemoteComboBox.Text = _settings.TfsRemote;
        }

        private void InitializePull()
        {
            switch(_settings.PullSetting)
            {
                case PullSetting.Pull:
                    PullRadioButton.Checked = true;
                    break;
                case PullSetting.Rebase:
                    RebaseRadioButton.Checked = true;
                    break;
                case PullSetting.Fetch:
                    FetchRadioButton.Checked = true;
                    break;
            }

            SetPullButtonEnabledState();
        }

        private void MergeOptionCheckedChanged(object sender, EventArgs e)
        {
            SetPullButtonEnabledState();
        }

        private void SetPullButtonEnabledState()
        {
            PullButton.Enabled = PullRadioButton.Checked || RebaseRadioButton.Checked || FetchRadioButton.Checked;
        }

        private void PullButtonClick(object sender, EventArgs e)
        {
            if (PullRadioButton.Checked)
            {
                _settings.PullSetting = PullSetting.Pull;
                if (!_commands.StartGitTfsCommandProcessDialog("pull", "--remote " + TfsRemoteComboBox.Text))
                {
                    _commands.StartResolveConflictsDialog();
                }
            }
            else if (RebaseRadioButton.Checked)
            {
                _settings.PullSetting = PullSetting.Rebase;
                _commands.StartGitTfsCommandProcessDialog("fetch", "--remote " + TfsRemoteComboBox.Text);
                _commands.StartRebaseDialog("tfs/" + TfsRemoteComboBox.Text);
            }
            else if (FetchRadioButton.Checked)
            {
                _settings.PullSetting = PullSetting.Fetch;
                _commands.StartGitTfsCommandProcessDialog("fetch", "--remote " + TfsRemoteComboBox.Text);
            }
            this.Close();
        }

        private void InitializePush()
        {
            switch (_settings.PushSetting)
            {
                case PushSetting.Checkin:
                    CheckinRadioButton.Checked = true;
                    break;
                case PushSetting.Shelve:
                    ShelveRadioButton.Checked = true;
                    break;
                case PushSetting.RCheckin:
                    RCheckinRadioButton.Checked = true;
                    break;
            }

            SetPushButtonEnabledState();
        }

        private void PushOptionCheckedChanged(object sender, EventArgs e)
        {
            SetPushButtonEnabledState();
        }

        private void SetPushButtonEnabledState()
        {
            PushButton.Enabled = CheckinRadioButton.Checked || ShelveRadioButton.Checked || RCheckinRadioButton.Checked;
        }

        private void PushButtonClick(object sender, EventArgs e)
        {
            if (CheckinRadioButton.Checked)
            {
                _settings.PushSetting = PushSetting.Checkin;
                _commands.StartGitTfsCommandProcessDialog("checkintool", "--remote " + TfsRemoteComboBox.Text);
            }
            else if (ShelveRadioButton.Checked)
            {
                _settings.PushSetting = PushSetting.Shelve;
                var dialogResult = new ShelveDialog(_commands, _settings.ShelveSettings).ShowDialog();
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }
            else if (RCheckinRadioButton.Checked)
            {
                _settings.PushSetting = PushSetting.RCheckin;
                _commands.StartGitTfsCommandProcessDialog("rcheckin", "--remote " + TfsRemoteComboBox.Text);
            }
            this.Close();
        }

        private void TfsRemoteComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            _settings.TfsRemote = TfsRemoteComboBox.Text;
        }
    }
}
