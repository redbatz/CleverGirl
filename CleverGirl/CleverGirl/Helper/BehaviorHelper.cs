﻿using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverGirl.Helper
{
    public static class BehaviorHelper
    {
        // --- BEHAVIOR VARIABLE BELOW
        public static BehaviorVariableValue GetCachedBehaviorVariableValue(BehaviorTree bTree, BehaviorVariableName name)
        {
            return ModState.BehaviorVarValuesCache.GetOrAdd(name, GetBehaviorVariableValue(bTree, name));
        }

        // TODO: EVERYTHING SHOULD CONVERT TO CACHED CALL IF POSSIBLE
        public static BehaviorVariableValue GetBehaviorVariableValue(BehaviorTree bTree, BehaviorVariableName name)
        {

            if (ModState.RolePlayerBehaviorVarManager != null && ModState.RolePlayerGetBehaviorVar != null)
            {
                // Ask RolePlayer for the variable
                //getBehaviourVariable(AbstractActor actor, BehaviorVariableName name)
                return (BehaviorVariableValue)ModState.RolePlayerGetBehaviorVar.Invoke(ModState.RolePlayerBehaviorVarManager, new object[] { bTree.unit, name });
            }
            else
            {
                // Read it directly
                return GetBehaviorVariableValueDirectly(bTree, name);
            }

        }

        // TODO: EVERYTHING SHOULD CONVERT TO CACHED CALL IF POSSIBLE
        private  static BehaviorVariableValue GetBehaviorVariableValueDirectly(BehaviorTree bTree, BehaviorVariableName name)
        {
            //getBehaviourVariable(AbstractActor actor, BehaviorVariableName name)

            if (ModState.RolePlayerGetBehaviorVar != null)
            {
                // Ask RolePlayer for the variable
            }
            else
            {
                // Read it directly
            }


            BehaviorVariableValue behaviorVariableValue = bTree.unitBehaviorVariables.GetVariable(name);
            if (behaviorVariableValue != null)
            {
                return behaviorVariableValue;
            }

            Pilot pilot = bTree.unit.GetPilot();
            if (pilot != null)
            {
                BehaviorVariableScope scopeForAIPersonality = bTree.unit.Combat.BattleTechGame.BehaviorVariableScopeManager.GetScopeForAIPersonality(pilot.pilotDef.AIPersonality);
                if (scopeForAIPersonality != null)
                {
                    behaviorVariableValue = scopeForAIPersonality.GetVariableWithMood(name, bTree.unit.BehaviorTree.mood);
                    if (behaviorVariableValue != null)
                    {
                        return behaviorVariableValue;
                    }
                }
            }

            if (bTree.unit.lance != null)
            {
                behaviorVariableValue = bTree.unit.lance.BehaviorVariables.GetVariable(name);
                if (behaviorVariableValue != null)
                {
                    return behaviorVariableValue;
                }
            }

            if (bTree.unit.team != null)
            {
                Traverse bvT = Traverse.Create(bTree.unit.team).Field("BehaviorVariables");
                BehaviorVariableScope bvs = bvT.GetValue<BehaviorVariableScope>();
                behaviorVariableValue = bvs.GetVariable(name);
                if (behaviorVariableValue != null)
                {
                    return behaviorVariableValue;
                }
            }

            UnitRole unitRole = bTree.unit.DynamicUnitRole;
            if (unitRole == UnitRole.Undefined)
            {
                unitRole = bTree.unit.StaticUnitRole;
            }

            BehaviorVariableScope scopeForRole = bTree.unit.Combat.BattleTechGame.BehaviorVariableScopeManager.GetScopeForRole(unitRole);
            if (scopeForRole != null)
            {
                behaviorVariableValue = scopeForRole.GetVariableWithMood(name, bTree.unit.BehaviorTree.mood);
                if (behaviorVariableValue != null)
                {
                    return behaviorVariableValue;
                }
            }

            if (bTree.unit.CanMoveAfterShooting)
            {
                BehaviorVariableScope scopeForAISkill = bTree.unit.Combat.BattleTechGame.BehaviorVariableScopeManager.GetScopeForAISkill(AISkillID.Reckless);
                if (scopeForAISkill != null)
                {
                    behaviorVariableValue = scopeForAISkill.GetVariableWithMood(name, bTree.unit.BehaviorTree.mood);
                    if (behaviorVariableValue != null)
                    {
                        return behaviorVariableValue;
                    }
                }
            }

            behaviorVariableValue = bTree.unit.Combat.BattleTechGame.BehaviorVariableScopeManager.GetGlobalScope().GetVariableWithMood(name, bTree.unit.BehaviorTree.mood);
            if (behaviorVariableValue != null)
            {
                return behaviorVariableValue;
            }

            return DefaultBehaviorVariableValue.GetSingleton();
        }
    }
}
