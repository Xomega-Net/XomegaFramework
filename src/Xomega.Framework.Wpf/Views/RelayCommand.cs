﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Generic command for relaying actions to view models
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Func<T, Task> _executeAsync;
        readonly Predicate<T> _canExecute;

        /// <summary>
        /// Constructs a relay command with the specified action
        /// </summary>
        /// <param name="execute">Action to execute</param>
        public RelayCommand(Action<T> execute) : this(execute, null) { }

        /// <summary>
        /// Constructs a relay command with the specified action
        /// </summary>
        /// <param name="executeAsync">Action to execute</param>
        public RelayCommand(Func<T, Task> executeAsync) : this(executeAsync, null) { }

        /// <summary>
        /// Constructs a relay command with the specified action and a check if command can be executed
        /// </summary>
        /// <param name="execute">Action to execute</param>
        /// <param name="canExecute">Delegate that determines if command can be executed</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute= execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Constructs a relay command with the specified action and a check if command can be executed
        /// </summary>
        /// <param name="executeAsync">Action to execute</param>
        /// <param name="canExecute">Delegate that determines if command can be executed</param>
        public RelayCommand(Func<T, Task> executeAsync, Predicate<T> canExecute)
        {
            if (executeAsync == null) throw new ArgumentNullException("executeAsync");
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed,
        /// this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter) { return _canExecute == null ? true : _canExecute((T)parameter); }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed,
        /// this object can be set to null.</param>
        public async void Execute(object parameter)
        {
            if (_executeAsync != null)
                await _executeAsync((T)parameter);
            else _execute((T)parameter);
        }
    }
}
