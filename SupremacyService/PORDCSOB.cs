// PORDCSOB.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace Supremacy.WCF
{
    internal sealed class PORDCSOB : DataContractSerializerOperationBehavior
    {
        public PORDCSOB(OperationDescription od) : base(od) { }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject,
                true , DataContractSurrogate);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes, MaxItemsInObjectGraph, IgnoreExtensionDataObject,
                true , DataContractSurrogate);
        }
    }

}
