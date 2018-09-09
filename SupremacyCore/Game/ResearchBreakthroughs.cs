// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Game;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremacy.Game

{
    [Serializable]
    public class  ReserchBreakthroughs : GameEngine
    {      
        bool m_traceNebulaProtomater = true;

        if (m_traceNebulaProtomater)
        {
            //GameLog.Print("Colony = {0}, population after = {1}, health after = {2}", targetColonyId, GameContext.Current.Universe.Get<Colony>(targetColonyId).Population.CurrentValue, GameContext.Current.Universe.Get<Colony>(targetColonyId).Health.CurrentValue);
        }

        GameContext.Current.Universe.UpdateSectors();

                    return;
                
            
        
    
    }
