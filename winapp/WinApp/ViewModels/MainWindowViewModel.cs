// Copyright (c) 2021 Inaba
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Livet;
using Livet.Commands;

using WinApp.Models;

namespace WinApp.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private readonly Model Model;

        #region public string UserID
        private string _UserID;

        public string UserID
        {
            get => _UserID;
            private set
            {
                if (_UserID == value)
                    return;
                _UserID = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region public string UserName
        private string _UserName;

        public string UserName
        {
            get => _UserName;
            private set
            {
                if (_UserName == value)
                    return;
                _UserName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region public string UserDescription
        private string _UserDescription;

        public string UserDescription
        {
            get => _UserDescription;
            private set
            {
                if (_UserDescription == value)
                    return;
                _UserDescription = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region public string LastLocation
        private string _LastLocation;

        public string LastLocation
        {
            get => _LastLocation;
            private set
            {
                if (_LastLocation == value)
                    return;
                _LastLocation = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region public bool? Running
        private bool? _Running = null;

        public bool? Running
        {
            get => _Running;
            private set
            {
                if (_Running == value)
                    return;
                _Running = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region public bool SignedIn
        private bool _SignedIn = false;

        public bool SignedIn
        {
            get => _SignedIn;
            private set
            {
                if (_SignedIn == value)
                    return;
                _SignedIn = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public MainWindowViewModel(Model model)
        {
            Model = model;

            Model.StateChanged += (s, _) =>
            {
                var m = s as Model;
                switch (m.State)
                {
                    case State.Offline:
                        Running = null;
                        SignedIn = false;
                        break;
                    case State.Ready:
                        Running = false;
                        SignedIn = true;
                        UserID = m.UserID;
                        UserName = m.UserName;
                        UserDescription = m.UserDescription;
                        break;
                    case State.Runnig:
                        Running = true;
                        SignedIn = true;
                        break;
                }

                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
                SignInCommand.RaiseCanExecuteChanged();
                SignOutCommand.RaiseCanExecuteChanged();
                SignUpCommand.RaiseCanExecuteChanged();
            };

            Model.LocationChanged += (s, _) =>
            {
                var l = (s as Model).LastLocation;
                LastLocation = $"{l.Name} @ {l.Timestamp}";
            };
        }

        public async void Initialize()
        {
            await Model.Initialize();

            UserID = Model.UserID;
            UserName = Model.UserName;
            UserDescription = Model.UserDescription;
        }

        #region public ListenerCommand<string> SignInCommand
        private ListenerCommand<string> _SignInCommand;

        public ListenerCommand<string> SignInCommand
        {
            get
            {
                if (_SignInCommand == null)
                {
                    _SignInCommand = new ListenerCommand<string>(SignIn, CanSignIn);
                }
                return _SignInCommand;
            }
        }

        public bool CanSignIn() => !SignedIn;

        public async void SignIn(string parameter)
            => await Model.SignIn(parameter);
        #endregion

        #region public ListenerCommand<(string, string, string)> SignUpCommand
        private ListenerCommand<(string, string, string)> _SignUpCommand;

        public ListenerCommand<(string, string, string)> SignUpCommand
        {
            get
            {
                if (_SignUpCommand == null)
                {
                    _SignUpCommand = new ListenerCommand<(string, string, string)>(SignUp, CanSignUp);
                }
                return _SignUpCommand;
            }
        }

        public bool CanSignUp() => !SignedIn;

        public async void SignUp((string id, string name, string desc) parameter)
            => await Model.SignUp(parameter.id, parameter.name, parameter.desc);
        #endregion

        #region public ViewModelCommand SignOutCommand
        private ViewModelCommand _SignOutCommand;

        public ViewModelCommand SignOutCommand
        {
            get
            {
                if (_SignOutCommand == null)
                {
                    _SignOutCommand = new ViewModelCommand(SignOut, CanSignOut);
                }
                return _SignOutCommand;
            }
        }

        public bool CanSignOut() => SignedIn && !(Running ?? false);

        public void SignOut() => Model.SignOut();
        #endregion

        #region public ViewModelCommand StartCommand
        private ViewModelCommand _StartCommand;

        public ViewModelCommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new ViewModelCommand(Start, CanStart);
                }
                return _StartCommand;
            }
        }

        public bool CanStart() => !(Running ?? true);

        public async void Start() => await Model.Start();
        #endregion

        #region public ViewModelCommand StopCommand
        private ViewModelCommand _StopCommand;

        public ViewModelCommand StopCommand
        {
            get
            {
                if (_StopCommand == null)
                {
                    _StopCommand = new ViewModelCommand(Stop, CanStop);
                }
                return _StopCommand;
            }
        }

        public bool CanStop() => Running ?? false;

        public void Stop() => Model.Stop();
        #endregion
    }
}
