// IResponse.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;

namespace Supremacy.Diplomacy
{
    public interface IResponse : IDiplomaticExchange
    {
        TurnNumber TurnSent { get; }
        ResponseType ResponseType { get; }
        IProposal Proposal { get; }
        IProposal CounterProposal { get; }
        Tone Tone { get; }
    }

    [Serializable]
    public class Response : IResponse, IOwnedDataSerializable, IDiplomaticExchange
    {
        private ResponseType _responseType;
        private IProposal _proposal;
        private IProposal _counterProposal;
        private TurnNumber _turnSent;

        public Response(ResponseType responseType, [NotNull] IProposal proposal, [CanBeNull] IProposal counterProposal = null, TurnNumber turnSent = default(TurnNumber))
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            _turnSent = turnSent.IsUndefined ? GameContext.Current.TurnNumber + 1 : turnSent;
            _responseType = responseType;
            _proposal = proposal;
            _counterProposal = counterProposal;
            _turnSent = turnSent;
        }

        #region Implementation of IResponse

        public TurnNumber TurnSent
        {
            get { return _turnSent; }
        }

        public Civilization Sender
        {
            get { return _proposal.Recipient; }
        }

        public Civilization Recipient
        {
            get { return _proposal.Sender; }
        }

        public ResponseType ResponseType
        {
            get { return _responseType; }
        }

        public IProposal Proposal
        {
            get { return _proposal; }
        }

        public IProposal CounterProposal
        {
            get { return _counterProposal; }
        }

        public Tone Tone
        {
            get { return Tone.Calm; }
        }

        #endregion

        #region Implementation of IOwnedDataSerializable

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _turnSent = reader.ReadInt32();
            _responseType = (ResponseType)reader.ReadByte();
            _proposal = reader.Read<IProposal>();
            _counterProposal = reader.Read<IProposal>();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_turnSent);
            writer.Write((byte)_responseType);
            writer.WriteObject(_proposal);
            writer.WriteObject(_counterProposal);
        }

        #endregion
    }

    public static class ResponseExtensions
    {
        public static bool IsCounterProposal(this IResponse response)
        {
            return TestResponseType(response, ResponseType.Counter);
        }

        public static bool IsAcceptance(this IResponse response)
        {
            return TestResponseType(response, ResponseType.Accept);
        }

        public static bool IsRejection(this IResponse response)
        {
            return TestResponseType(response, ResponseType.Reject);
        }

        public static bool IsValid(this IResponse response)
        {
            if (response == null)
                return false;
            return (response.ResponseType != ResponseType.NoResponse);
        }

        private static bool TestResponseType(IResponse response, ResponseType responseType)
        {
            if (response == null)
                return false;
            return (response.ResponseType == responseType);
        }
    }
}