using Photon.Pun;
using UnityEngine;

public class Push : Skill
{
    public float pushPower;
    bool canPush = true;
    float coolTime = 0f;
        
    private void Update()
    {
        if (!canPush)
        {
            coolTime += Time.deltaTime;

            if (coolTime >= 3f)
            {                
                canPush = true;
                coolTime = 0f;                
            }
        }
    }

    public override void ActiveSkill()
    {
        base.ActiveSkill();

        if (canPush)
        {
            if (target != null)
            {
                transform.parent.LookAt(target.transform.position);
                photonView.RPC("PushTarget", RpcTarget.All, target.GetPhotonView().ViewID);
                target = null;
            }
            else
            {
                print("targetView is Not Set!");
                // 그냥 애니메이션만 출력
            }

            canPush = false;
        }
    }

    [PunRPC]
    public void PushTarget(int targetViewID)
    {
        OXPlayer target = PhotonView.Find(targetViewID).GetComponent<OXPlayer>();

        if (!target.isDefeated)
        {
            target.BePushed(transform.parent.position, pushPower);
        }
        else
        {
            // 애니메이션만 출력
        }
    }
}
