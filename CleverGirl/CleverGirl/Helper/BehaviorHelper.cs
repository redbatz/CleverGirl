﻿using System;
using System.Diagnostics;

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

            BehaviorVariableValue bhVarVal = null;
            if (ModState.RolePlayerBehaviorVarManager != null && ModState.RolePlayerGetBehaviorVar != null)
            {
                // Ask RolePlayer for the variable
                //getBehaviourVariable(AbstractActor actor, BehaviorVariableName name)
                Mod.Log.Trace?.Write($"Pulling BehaviorVariableValue {name} from RolePlayer for unit: {bTree.unit.DistinctId()}.");
                bhVarVal = (BehaviorVariableValue)ModState.RolePlayerGetBehaviorVar.Invoke(ModState.RolePlayerBehaviorVarManager, new object[] { bTree.unit, name });
            }

            if (bhVarVal == null)
            {
                // RolePlayer does not return the vanilla value if there's no configuration for the actor. We need to check that we're null here to trap that edge case.
                // Also, if RolePlayer isn't configured we need to read the value. 
                Mod.Log.Trace?.Write($"Pulling BehaviorVariableValue {name} from Vanilla for unit: {bTree.unit.DistinctId()}.");
                bhVarVal = GetBehaviorVariableValueDirectly(bTree, name);
            }

            Mod.Log.Trace?.Write($" Value of {name} with type {bhVarVal.type} is {bhVarVal.GetValueString()}");
            return bhVarVal;

        }

        private static string GetValueString(this BehaviorVariableValue behaviorVariableValue)
        {
            switch (behaviorVariableValue.type)
            {
                case BehaviorVariableValue.BehaviorVariableType.Float:
                    return behaviorVariableValue.FloatVal.ToString();
                case BehaviorVariableValue.BehaviorVariableType.Int:
                    return behaviorVariableValue.IntVal.ToString();
                case BehaviorVariableValue.BehaviorVariableType.Bool:
                    return behaviorVariableValue.BoolVal.ToString();
                case BehaviorVariableValue.BehaviorVariableType.String:
                    return behaviorVariableValue.StringVal;
                default:
                    return "Undefined";
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
                BehaviorVariableScope bvs = bTree.unit.team.BehaviorVariables;
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
