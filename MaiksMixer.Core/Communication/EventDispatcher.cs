using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaiksMixer.Core.Communication.Models;
using MaiksMixer.Core.Logging;

namespace MaiksMixer.Core.Communication
{
    /// <summary>
    /// Delegate for event handlers.
    /// </summary>
    /// <param name="eventMessage">The event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public delegate Task EventHandlerDelegate(EventMessage eventMessage);
    
    /// <summary>
    /// Dispatches events to registered handlers.
    /// </summary>
    public class EventDispatcher
    {
        private readonly Dictionary<string, List<EventHandlerDelegate>> _handlers;
        
        /// <summary>
        /// Initializes a new instance of the EventDispatcher class.
        /// </summary>
        public EventDispatcher()
        {
            _handlers = new Dictionary<string, List<EventHandlerDelegate>>(StringComparer.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Registers a handler for an event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The handler for the event.</param>
        public void RegisterHandler(string eventName, EventHandlerDelegate handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));
            
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            if (!_handlers.TryGetValue(eventName, out var handlers))
            {
                handlers = new List<EventHandlerDelegate>();
                _handlers[eventName] = handlers;
            }
            
            handlers.Add(handler);
            LogManager.Info($"Registered handler for event: {eventName}");
        }
        
        /// <summary>
        /// Unregisters a handler for an event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="handler">The handler to unregister.</param>
        /// <returns>True if the handler was unregistered; otherwise, false.</returns>
        public bool UnregisterHandler(string eventName, EventHandlerDelegate handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));
            
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            
            if (_handlers.TryGetValue(eventName, out var handlers))
            {
                bool result = handlers.Remove(handler);
                
                if (result)
                {
                    LogManager.Info($"Unregistered handler for event: {eventName}");
                    
                    // Remove the event from the dictionary if there are no more handlers
                    if (handlers.Count == 0)
                    {
                        _handlers.Remove(eventName);
                    }
                }
                
                return result;
            }
            
            return false;
        }
        
        /// <summary>
        /// Unregisters all handlers for an event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <returns>True if any handlers were unregistered; otherwise, false.</returns>
        public bool UnregisterAllHandlers(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));
            
            bool result = _handlers.Remove(eventName);
            
            if (result)
            {
                LogManager.Info($"Unregistered all handlers for event: {eventName}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Dispatches an event to its registered handlers.
        /// </summary>
        /// <param name="eventMessage">The event to dispatch.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DispatchEventAsync(EventMessage eventMessage)
        {
            if (eventMessage == null)
                throw new ArgumentNullException(nameof(eventMessage));
            
            if (string.IsNullOrEmpty(eventMessage.EventName))
            {
                LogManager.Warn("Cannot dispatch event with null or empty event name.");
                return;
            }
            
            if (_handlers.TryGetValue(eventMessage.EventName, out var handlers))
            {
                LogManager.Info($"Dispatching event: {eventMessage.EventName} to {handlers.Count} handlers");
                
                // Create a list of tasks for each handler
                var tasks = new List<Task>(handlers.Count);
                
                foreach (var handler in handlers)
                {
                    try
                    {
                        tasks.Add(handler(eventMessage));
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error($"Error invoking handler for event {eventMessage.EventName}: {ex.Message}", ex);
                    }
                }
                
                // Wait for all handlers to complete
                await Task.WhenAll(tasks);
            }
            else
            {
                LogManager.Warn($"No handlers registered for event: {eventMessage.EventName}");
            }
        }
        
        /// <summary>
        /// Gets a list of all registered event names.
        /// </summary>
        /// <returns>A list of registered event names.</returns>
        public IEnumerable<string> GetRegisteredEvents()
        {
            return _handlers.Keys;
        }
        
        /// <summary>
        /// Gets the number of handlers registered for an event.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <returns>The number of handlers registered for the event.</returns>
        public int GetHandlerCount(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name cannot be null or empty.", nameof(eventName));
            
            if (_handlers.TryGetValue(eventName, out var handlers))
            {
                return handlers.Count;
            }
            
            return 0;
        }
        
        /// <summary>
        /// Clears all registered handlers.
        /// </summary>
        public void ClearHandlers()
        {
            _handlers.Clear();
            LogManager.Info("Cleared all event handlers");
        }
    }
}
