﻿using Harmony;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NitroxModel.Helper.GameLogic.TransientLocalObjectManager;

namespace NitroxPatcher.Patches
{
    public class ConstructorInput_OnCraftingBegin_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ConstructorInput);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly OpCode INJECTION_OPCODE = OpCodes.Callvirt;
        public static readonly object INJECTION_OPERAND = typeof(Constructor).GetMethod("SendBuildBots", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(GameObject) }, null);

        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            Validate.NotNull(INJECTION_OPERAND);

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.opcode.Equals(INJECTION_OPCODE) && instruction.operand.Equals(INJECTION_OPERAND))
                {
                    /*
                     * TransientLocalObjectManager.Add(TransientLocalObjectManager.TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT, gameObject);
                     */
                    yield return new ValidatedCodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Ldloc_0);
                    yield return new ValidatedCodeInstruction(OpCodes.Call, typeof(TransientLocalObjectManager).GetMethod("Add", BindingFlags.Static | BindingFlags.Public, null, new Type[] { TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT.GetType(), typeof(object) }, null));
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchTranspiler(harmony, TARGET_METHOD);
        }
    }
}

