﻿// 
// ScriptedEventDatabase.cs
// 
// Copyright (c) 2013-2013 Mike Strobel
// 
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
// 
// All other rights reserved.
// 

using System;
using System.Xaml;

using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.Resources;
using Supremacy.VFS;

namespace Supremacy.Scripting
{
    public sealed class ScriptedEventDatabase : KeyedCollectionBase<string, EventDefinition>
    {
        public ScriptedEventDatabase()
            : base(o => o.EventID, StringComparer.OrdinalIgnoreCase) { }

        public static ScriptedEventDatabase Load()
        {
            GameContext gameContext = GameContext.Current;
            if (gameContext == null)
            {
                gameContext = GameContext.Create(GameOptionsManager.LoadDefaults(), false);
            }

            GameContext.PushThreadContext(gameContext);

            try
            {

                if (!ResourceManager.VfsService.TryGetFileInfo(new Uri("vfs:///Resources/Data/ScriptedEvents.xaml"), out IVirtualFileInfo fileInfo))
                {
                    return null;
                }

                if (!fileInfo.Exists)
                {
                    return null;
                }

                using (System.IO.Stream stream = fileInfo.OpenRead())
                {
                    return (ScriptedEventDatabase)XamlServices.Load(stream);
                }
            }
            finally
            {
                _ = GameContext.PopThreadContext();
            }
        }
    }
}