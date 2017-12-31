// EnumHelper.h
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

#pragma once

#include "DoubleKeyedSet.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace System::Globalization;
using namespace System::Linq;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;

using namespace Supremacy::Types;

namespace Supremacy
{
    namespace Utility
    {
		[Extension]
		public ref class EnumHelper sealed abstract
        {
        public:
            generic <typename T> where T: Enum
            static int GetValueOrdinal(T value)
            {
                array<T>^ values = ((array<T>^) Enum::GetValues(T::typeid));
                return values->IndexOf(values, value);
            }

            generic <typename T> where T: Enum
            static array<T>^ GetValues()
            {
                return ((array<T>^) Enum::GetValues(T::typeid));
            }

            generic <typename T> where T: Enum
            static array<String^>^ GetNames()
            {
                return ((array<String^>^) Enum::GetNames(T::typeid));
            }

            generic <typename T> where T: Enum
            static bool IsDefined(T value)
            {
                return Enum::IsDefined(T::typeid, value);
            }

            generic <typename T> where T: Enum
            static bool IsDefined(T value, [Out] int %ordinal)
            {
                ordinal = Array::IndexOf(GetValues<T>(), value);
                return (ordinal >= 0);
            }

            generic <typename T> where T: Enum
            [Extension]
            static bool IsSet(T source, T value)
            {
                UInt64 rawSouce = (UInt64)source;
                UInt64 rawValue = (UInt64)value;

                return (rawSouce & rawValue) == rawValue;
            }

            generic <typename T> where T: Enum
            [Extension]
            static bool MatchAttribute(T source, Attribute^ attribute)
            {
                bool result;
                if (!EnumHelper::EnumAttributeMatchCache->TryGetValue(source, attribute, result))
                {
                    result = false;
                    for each (Attribute^ customAttribute in Attribute::GetCustomAttributes(source->GetType()->GetField(source->ToString()), attribute->GetType()))
                    {
                        if (attribute->Match(customAttribute))
                        {
                            result = true;
                            break;
                        }
                    }
                    EnumHelper::EnumAttributeMatchCache[source, attribute] = result;
                }
                return result;
            }

            generic <typename TEnum, typename TAttribute>
                where TEnum: Enum
                where TAttribute : Attribute
            [Extension]
            static TAttribute GetAttribute(TEnum enumValue)
            {
                int ordinal;

                if (!IsDefined(enumValue, ordinal))
                    return TAttribute();

                AttributeCollection^ attributes;

                if (!EnumValueAttributeCache->TryGetItem(enumValue, attributes))
                {
                    String^ name = GetNames<TEnum>()[ordinal];
                    FieldInfo^ field = (TEnum::typeid)->GetField(name);

                    attributes = gcnew AttributeCollection(
                        Enumerable::ToArray(
                            Enumerable::Cast<Attribute^>(
                                field->GetCustomAttributes(false))));

                    attributes = EnumValueAttributeCache->GetOldest(enumValue, attributes);
                }

                return Enumerable::FirstOrDefault(Enumerable::OfType<TAttribute>(attributes));
            }

            generic <typename T> where T: value class, Enum
            static Nullable<T> Parse(String^ value)
            {
                T result;
                if (TryParse(value, false, result))
                    return result;
                return Nullable<T>();
            }

            generic <typename T> where T: value class, Enum
            static Nullable<T> Parse(String^ value, bool ignoreCase)
            {
                T result;
                if (TryParse(value, ignoreCase, result))
                    return result;
                return Nullable<T>();
            }

            generic <typename T> where T: value class, Enum
            static bool TryParse(String^ value, [Out] T %result)
            {
                return TryParse(value, false, result);
            }

            generic <typename T> where T: value class, Enum
            static bool TryParse(String^ value, bool ignoreCase, [Out] T %result)
            {
                result = T();

                if (!(T::typeid)->IsEnum)
                    return false;

                if (value == nullptr)
                    return false;

                value = value->Trim();

                if (value->Length == 0)
                    return false;

                try
                {
                    unsigned long long resultValue = 0L;
                    if ((Char::IsDigit(value[0]) || (value[0] == '-')) || (value[0] == '+'))
                    {
                        Type^ underlyingType = Enum::GetUnderlyingType(T::typeid);
                        try
                        {
                            Object^ convertedValue = Convert::ChangeType(value, underlyingType, CultureInfo::InvariantCulture);
                            result = (T)convertedValue;
                            return true;
                        }
                        catch (FormatException^)
                        {
                            return false;
                        }
                    }

                    array<String^>^ flagsArray = value->Split(EnumSeperators);
                    HashEntry^ hashEntry = GetHashEntry<T>();

                    array<String^>^ names = hashEntry->Names;
                    for (int i = 0; i < flagsArray->Length; i++)
                    {
                        flagsArray[i] = flagsArray[i]->Trim();

                        bool valueFound = false;

                        for (int j = 0; j < names->Length; j++)
                        {
                            if (ignoreCase)
                            {
                                if (!String::Equals(names[j], flagsArray[i], StringComparison::OrdinalIgnoreCase))
                                    continue;
                            }
                            else if (!String::Equals(names[j], flagsArray[i], StringComparison::Ordinal))
                            {
                                continue;
                            }
                            
                            unsigned long long enumValue = hashEntry->Values[j];
                            resultValue |= enumValue;
                            valueFound = true;
                            break;
                        }

                        if (valueFound)
                            continue;

                        return false;
                    }

                    result = (T)Convert::ChangeType(resultValue, Enum::GetUnderlyingType(T::typeid));
                    return true;
                }
                catch (Exception^)
                {
                    result = T();
                    return false;
                }
            }

            generic <typename T> where T: value class, Enum
            static T ParseOrGetDefault(String^ value)
            {
                return ParseOrGetDefault<T>(value, false);
            }

            generic <typename T> where T: value class, Enum
            static T ParseOrGetDefault(String^ value, bool ignoreCase)
            {
                if (String::IsNullOrEmpty(value))
                    return T();
                return (T)Enum::Parse(T::typeid, value->Trim(), ignoreCase);
            }

            generic <typename T> where T: value class, Enum
            [Extension]
            static String^ ToStringCached(T source)
            {
                String^ result;
                if (!EnumHelper::EnumStringCache->TryGetValue(source, result))
                {
                    result = source->ToString();
                    EnumHelper::EnumStringCache[source] = result;
                }
                return result;
            }

            generic <typename T> where T: value class, Enum
            static array<T>^ GetRandomizedValues()
            {
                return EnumHelper::GetRandomizedValues<T>(gcnew System::Random());
            }

            generic <typename T> where T: value class, Enum
            static array<T>^ GetRandomizedValues(System::Random^ randomGenerator)
            {
                array<T>^ values = GetValues<T>();
                
                if (randomGenerator == nullptr)
                    randomGenerator = gcnew System::Random();

                for (int i = values->Length - 1 ; i >= 1; i--)
                {
                    int j = randomGenerator->Next(i + 1);
                    T temp = values[i];
                    values[i] = values[j];
                    values[j] = temp;
                }

                return values;
            }

        private:
            generic <typename T> where T: Enum
            static void CheckCompatibleEnum(T value)
            {
                if (!TypeDescriptor::GetConverter(value)->CanConvertTo(unsigned long long::typeid))
                {
                    throw gcnew InvalidOperationException(
                        String::Format(
                            "Type '{0}' is not assignable to type '{1}'.",
                            value->GetType()->FullName,
                            Enum::typeid->FullName)) ;
                }
            }

            ref class HashEntry
            {
            public:
                initonly array<String^>^ Names;
                initonly array<UInt64>^ Values;

                HashEntry(array<String^>^ names, array<UInt64>^ values)
                    : Names(names),
                      Values(values) {}
            };

            static unsigned long long ToUInt64(Object^ value)
            {
                if (value == nullptr)
                    throw gcnew ArgumentNullException("value");

                switch (Convert::GetTypeCode(value))
                {
                    case TypeCode::SByte:
                    case TypeCode::Int16:
                    case TypeCode::Int32:
                    case TypeCode::Int64:
                        return (unsigned long long)Convert::ToInt64(value, CultureInfo::InvariantCulture);

                    case TypeCode::Byte:
                    case TypeCode::UInt16:
                    case TypeCode::UInt32:
                    case TypeCode::UInt64:
                        return Convert::ToUInt64(value, CultureInfo::InvariantCulture);
                }

                throw gcnew InvalidOperationException("Invalid type: " + value->GetType()->Name);
            }

            generic <typename T> where T: Enum
            static HashEntry^ GetHashEntry()
            {
                HashEntry^ entry = (HashEntry^)EnumHelper::FieldInfoHash[T::typeid];
                if (entry == nullptr)
                {
                    if (FieldInfoHash->Count > 100)
                        FieldInfoHash->Clear();

                    array<T>^ enumValues;
                    array<unsigned long long>^ values;
                    array<String^>^ names;

                    if ((T::typeid)->BaseType == Enum::typeid)
                    {
                        enumValues = GetValues<T>();
                        names = GetNames<T>();
                        values = gcnew array<unsigned long long>(enumValues->Length);

                        for (int i = 0; i < enumValues->Length; i++)
                            values[i] = Convert::ToUInt64(enumValues[i]);
                    }
                    else
                    {
                        array<FieldInfo^>^ fields = (T::typeid)->GetFields(BindingFlags::Public | BindingFlags::Static);
                        
                        values = gcnew array<unsigned long long>(fields->Length);
                        names = gcnew array<String^>(fields->Length);
                        
                        for (int i = 0; i < fields->Length; i++)
                        {
                            names[i] = fields[i]->Name;
                            values[i] = ToUInt64(fields[i]->GetValue(nullptr));
                        }

                        for (int j = 1; j < values->Length; j++)
                        {
                            int index = j;
                            String^ name = names[j];
                            unsigned long long value = values[j];
                            bool setValue = false;

                            while (values[index - 1] > value)
                            {
                                names[index] = names[index - 1];
                                values[index] = values[index - 1];
                                index--;
                                setValue = true;
                                if (index == 0)
                                    break;
                            }

                            if (!setValue)
                                continue;

                            names[index] = name;
                            values[index] = value;
                        }
                    }

                    entry = gcnew HashEntry(names, values);
                    FieldInfoHash[T::typeid] = entry;
                }
                return entry;
            }
            
            static EnumHelper()
            {
                EnumAttributeMatchCache = gcnew DoubleKeyedSet<Enum^, Attribute^, bool>();
                EnumStringCache = gcnew Dictionary<Enum^, String^>();
                EnumValueAttributeCache = gcnew TvdP::Collections::Cache<Enum^, AttributeCollection^>();
                FieldInfoHash = Hashtable::Synchronized(gcnew Hashtable());
                EnumSeperators = gcnew array<wchar_t>(',');
            }

            static initonly DoubleKeyedSet<Enum^, Attribute^, bool>^ EnumAttributeMatchCache;
            static initonly TvdP::Collections::Cache<Enum^, AttributeCollection^>^ EnumValueAttributeCache;
            static initonly Dictionary<Enum^, String^>^ EnumStringCache;

            static initonly array<wchar_t>^ EnumSeperators;
            static initonly Hashtable^ FieldInfoHash;
        };
    }
}