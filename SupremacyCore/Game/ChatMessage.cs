// ChatMessage.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

namespace Supremacy.Game
{
    /// <summary>
    /// A chat message sent from one player to another.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        private readonly IPlayer _sender;
        private readonly IPlayer _recipient;
        private readonly string _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        public ChatMessage(IPlayer sender, string message)
            : this(sender, message, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessage"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="message">The message.</param>
        /// <param name="recipient">The recipient.</param>
        public ChatMessage(IPlayer sender, string message, IPlayer recipient)
        {
            _sender = sender;
            _message = message;
            _recipient = recipient;
        }

        /// <summary>
        /// Gets the sender.
        /// </summary>
        /// <value>The sender.</value>
        public IPlayer Sender
        {
            get { return _sender; }
        }

        /// <summary>
        /// Gets the recipient.
        /// </summary>
        /// <value>The recipient.</value>
        public IPlayer Recipient
        {
            get { return _recipient; }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return _message; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ChatMessage"/> is global message.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="ChatMessage"/> is global message; otherwise, <c>false</c>.
        /// </value>
        public bool IsGlobalMessage
        {
            get { return (_recipient == null); }
        }
    }
}
