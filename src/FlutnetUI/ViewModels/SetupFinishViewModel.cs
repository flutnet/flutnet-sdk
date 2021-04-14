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

using System.Reactive;
using Flutnet.Cli.DTO;
using FlutnetUI.Models;
using ReactiveUI;

namespace FlutnetUI.ViewModels
{
    public class SetupFinishViewModel : AdvancedWizardPageViewModel
    {
        public SetupFinishViewModel(SetupResult result, AdvancedWizardPageViewModel previousPage, IScreen screen = null) : base("setup_finish", screen)
        {
            Button1Visible = false;
            Button2Visible = false;
            Button3Text = "Finish";
            IsFinishPage = true;

            Button3Command = ReactiveCommand.CreateFromObservable(() =>
            {
                var closeWizard = (HostScreen as SetupWindowViewModel)?.CloseWizard;
                return closeWizard?.Handle(Unit.Default);
            });

            if (result.OperationCanceled)
            {
                Title = "Setup Aborted";
                Text = "Setup Aborted";
                return;
            }

            if (result.OperationFailed)
            {
                Title = "Setup Failed";
                Text = Properties.Resources.Setup_Finish_UnexpectedError_Message;
                return;
            }

            Title = "Setup Completed";

            if (previousPage is SetupCheckFlutterViewModel || previousPage is SetupCheckFlutterErrorViewModel)
            {
                FlutterDiagOutArg operationResult = (FlutterDiagOutArg) result.OperationResult;

                string txt = string.Empty;
                switch (operationResult.Compatibility)
                {
                    case FlutterCompatibility.Supported:
                        txt = Properties.Resources.Setup_Finish_Congratulations_v1_Message;
                        break;

                    case FlutterCompatibility.SupportNotGuaranteed:
                        txt = !string.IsNullOrEmpty(operationResult.NextSupportedVersion)
                            ? Properties.Resources.Setup_Finish_LimitedSupport_v1_Message
                            : Properties.Resources.Setup_Finish_LimitedSupport_v2_Message;
                        break;

                    case FlutterCompatibility.NotSupported:
                        txt = Properties.Resources.Setup_Finish_NoCompatibility_Message;
                        break;
                }

                Text = SetupWindowViewModel.ReplaceText(txt, operationResult.InstalledVersion, operationResult.LatestSupportedVersion);
            }
            else
            {
                Text = Properties.Resources.Setup_Finish_Congratulations_v2_Message;
            }
        }

        public string StyleSheet => Properties.Resources.Setup_Styles;

        public string Text { get; }

    }
}