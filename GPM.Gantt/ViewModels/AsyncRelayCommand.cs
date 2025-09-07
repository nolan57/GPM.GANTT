using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPM.Gantt.ViewModels
{
    /// <summary>
    /// An ICommand implementation for asynchronous operations with built-in reentrancy guard and optional cancellation.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<CancellationToken, Task> _executeAsync;
        private readonly Func<bool>? _canExecute;
        private CancellationTokenSource? _cts;
        private bool _isRunning;

        public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns true while the command is executing.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Explicitly requests WPF to requery CanExecute for this command.
        /// This is a convenience wrapper around CommandManager.InvalidateRequerySuggested().
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object? parameter)
        {
            return !_isRunning && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            _cts = new CancellationTokenSource();
            IsRunning = true;
            try
            {
                await _executeAsync(_cts.Token).ConfigureAwait(false);
            }
            finally
            {
                IsRunning = false;
                _cts.Dispose();
                _cts = null;
            }
        }

        /// <summary>
        /// Requests cancellation of the running operation, if any.
        /// </summary>
        public void Cancel()
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
        }
    }
}