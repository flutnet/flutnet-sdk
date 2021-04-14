// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Reactive.Linq;
using System.Text;
using Flutnet.Cli.DTO;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class SetupCheckFlutterErrorViewModel : AdvancedWizardPageViewModel
    {
        FlutterDiagOutArg _checkResult;

        public SetupCheckFlutterErrorViewModel(FlutterDiagOutArg checkResult, IScreen screen = null) : base("setup_check_flutter_err", screen)
        {
            _checkResult = checkResult;

            Button1Visible = true;
            Button1Text = "Check again";
            Button1Command = screen?.Router.NavigateBack;

            Button3Visible = true;
            Button3Text = "Assisted setup";
            Button3Command = ReactiveCommand.CreateFromTask(async () =>
            {
                await HostScreen.Router.Navigate.Execute(new SetupAssistedViewModel(_checkResult, HostScreen));
            });
                
            if (checkResult.Issues == FlutterIssues.SdkNotFound)
            {
                Title = "Flutter Not Detected";

                Text = SetupWindowViewModel.ReplaceText(Properties.Resources.Setup_Check_FlutterNotFound_Message, checkResult.InstalledVersion, checkResult.LatestSupportedVersion);

                Button2Visible = false;

                FlutterNotFound = true;
            }
            else
            {
                Title = "Flutter Installed with Errors";

                Text = Properties.Resources.Setup_Check_FlutterErrors_Message;

                Button2Visible = true;
                Button2Text = "Ignore and continue";
                Button2Command = ReactiveCommand.CreateFromTask(async () =>
                {
                    await HostScreen.Router.Navigate.Execute(new SetupFinishViewModel(new Models.SetupResult
                    {
                        OperationResult = _checkResult
                    }, this, HostScreen));
                });

                FlutterNotFound = false;

                StringBuilder sb = new StringBuilder();
                foreach (var kvp in _checkResult.DoctorErrors)
                {
                    sb.AppendLine(kvp.Key);
                    foreach (string line in kvp.Value.Split(Environment.NewLine))
                    {
                        sb.AppendFormat("    {0}", line);
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                }
                Errors = sb.ToString();
            }
        }

        public bool FlutterNotFound { get; }

        public string Errors { get; }

        public string StyleSheet => Properties.Resources.Setup_Styles;

        public string Text { get; }
    }
}