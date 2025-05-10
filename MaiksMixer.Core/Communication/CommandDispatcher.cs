using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Delegate for command handlers.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public delegate Task CommandHandlerDelegate(CommandMessage command);
    
    /// <summary>
    /// Dispatches commands to registered handlers.
    /// </summary>
    public class CommandDispatcher
    {
        private readonly Dictionary<string, CommandHandlerDelegate> _handlers;
        
        /// <summary>
        /// Initializes a new instance of the CommandDispatcher class.
        /// </summary>
        public CommandDispatcher()
        {
            _handlers = new Dictionary<string, CommandHandlerDelegate>(StringComparer.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Registers a handler for a command.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="handler">The handler for the command.</param>
        public void RegisterHandler(string commandName, CommandHandlerDelegate handler)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be null or empty.", nameof(commandName));
            
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            _handlers[commandName] = handler;
            LogManager.Info($"Registered handler for command: {commandName}");
        }
        
        /// <summary>
        /// Unregisters a handler for a command.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <returns>True if the handler was unregistered; otherwise, false.</returns>
        public bool UnregisterHandler(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be null or empty.", nameof(commandName));
            
            bool result = _handlers.Remove(commandName);
            
            if (result)
                LogManager.Info($"Unregistered handler for command: {commandName}");
            
            return result;
        }
        
        /// <summary>
        /// Dispatches a command to its registered handler.
        /// </summary>
        /// <param name="command">The command to dispatch.</param>
        /// <returns>True if the command was dispatched; otherwise, false.</returns>
        public async Task<bool> DispatchCommandAsync(CommandMessage command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            
            if (string.IsNullOrEmpty(command.Command))
            {
                LogManager.Warn("Cannot dispatch command with null or empty command name.");
                return false;
            }
            
            if (_handlers.TryGetValue(command.Command, out var handler))
            {
                try
                {
                    LogManager.Info($"Dispatching command: {command.Command}");
                    await handler(command);
                    return true;
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Error handling command {command.Command}: {ex.Message}", ex);
                    return false;
                }
            }
            
            LogManager.Warn($"No handler registered for command: {command.Command}");
            return false;
        }
        
        /// <summary>
        /// Gets a list of all registered command names.
        /// </summary>
        /// <returns>A list of registered command names.</returns>
        public IEnumerable<string> GetRegisteredCommands()
        {
            return _handlers.Keys;
        }
        
        /// <summary>
        /// Clears all registered handlers.
        /// </summary>
        public void ClearHandlers()
        {
            _handlers.Clear();
            LogManager.Info("Cleared all command handlers");
        }
    }
}
