using UnityEngine;
using System.Reflection;
using AIs;
using HarmonyLib;

public class OnePunch : Mod
{

    private static Harmony m_harmony;

    private const string ModName = "OnePunch";
    private const string HarmonyId = "com.jcallow.projects.onepunch";

    public void Start()
    {
        m_harmony = new Harmony(HarmonyId);
        var assembly = Assembly.GetExecutingAssembly();
        m_harmony.PatchAll(assembly);
        Debug.Log("Mod OnePunch has been loaded!");
        
    }

    public void OnModUnload()
    {
        Debug.Log(string.Format("Mod {0} has been unloaded!", ModName));
        m_harmony.UnpatchAll(HarmonyId);
    }
}

[HarmonyPatch(typeof(PlayerFist))]
class PlayerFistPatch
{

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    static void Awake(PlayerFist __instance)
    {
        Debug.Log("Awake upgraded fists");
        SphereCollider s = (SphereCollider)__instance.m_HandCollider;
        s.radius = 1.0f;
    }


    [HarmonyPrefix]
    [HarmonyPatch("OnTriggerEnter")]
    static void OnTriggerEnter(Collider other, PlayerFist __instance)
    {
        Rigidbody hand = __instance.gameObject.GetComponent<Rigidbody>();
        SphereCollider s = (SphereCollider)__instance.m_HandCollider;
        s.radius = 1.0f;

        AI component = other.gameObject.GetComponent<AI>();
        if (component != null)
        {
            if (component.m_BoxCollider == null)
            {
                component.SetupBoxCollider();
            }
            component.StartRagdoll();
            if (component.m_RagdollBones != null)
            {
                
                foreach (RagdollBone b in component.m_RagdollBones)
                {
                    Vector3 vel = Player.Get().transform.forward.normalized * 40;
                    b.m_Rigidbody.velocity = vel;
                }
            }
        }

    }
}

[HarmonyPatch(typeof(FistFightController), "GiveDamage")]
class FistFightControllerPatch
{
    static bool Prefix(FistFightController __instance, ref DamageInfo ___m_DamageInfo, Collider ___m_LeftHandCollider,
        Collider ___m_RightHandCollider, AI ai)
    {

        Debug.Log("Damage info: " + ___m_DamageInfo);
        ___m_DamageInfo.m_Damage = 100000;
        ___m_DamageInfo.m_Damager = __instance.gameObject;
        ___m_DamageInfo.m_HitDir = __instance.transform.forward;
        ___m_DamageInfo.m_Position = (___m_LeftHandCollider.enabled ? ___m_LeftHandCollider.bounds.center : ___m_RightHandCollider.bounds.center);
        if (ai.TakeDamage(___m_DamageInfo))
        {
            PlayerAudioModule.Get().PlayFistsHitSound();
        }
        ___m_LeftHandCollider.enabled = false;
        ___m_RightHandCollider.enabled = false;
        bool flag = true;
        if (ai && ai.m_ID == AI.AIID.ArmadilloThreeBanded && ai.m_GoalsModule != null && ai.m_GoalsModule.m_ActiveGoal != null && ai.m_GoalsModule.m_ActiveGoal.m_Type == AIGoalType.Hide)
        {
            flag = false;
        }
        if (flag)
        {
            Skill.Get<FistsSkill>().OnSkillAction(true);
        }
        return false;
    }
}