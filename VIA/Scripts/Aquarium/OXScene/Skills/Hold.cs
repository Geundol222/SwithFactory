using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;

public class Hold : Skill
{
    public float holdTime = 3f;
    public bool isCasting;

    protected override void Awake()
    {
        base.Awake();

        isCasting = false;
    }

    public override void ActiveSkill()
    {
        base.ActiveSkill();

        if (target != null)
        {
            ChangeHoldingProp(true);
            transform.parent.LookAt(target.transform.position);
            photonView.RPC("HoldTarget", RpcTarget.All, target.GetPhotonView().ViewID, photonView.ViewID);
            target = null;
        }
        else
        {
            print("targetView is Not Set!");
            ChangeHoldingProp(false);
            HoldingFail().Forget();
            // 잡기 실패 애니메이션
        }
    }

    async UniTaskVoid HoldingFail()
    {
        GetComponentInParent<PlayerController>().enabled = false;

        await UniTask.Delay(TimeSpan.FromSeconds(2f));

        GetComponentInParent<PlayerController>().enabled = true;
    }

    public void ChangeHoldingProp(bool isHolding)
    {
        GetComponentInParent<PlayerController>().isHolding = isHolding;
    }

    [PunRPC]
    public void HoldTarget(int targetViewID, int casterViewID)
    {
        OXPlayer target = PhotonView.Find(targetViewID).GetComponent<OXPlayer>();
        Hold casterHold = PhotonView.Find(casterViewID).GetComponent<Hold>();

        if (!target.isDefeated)
        {
            casterHold.isCasting = true;
            target.BeHolded(transform.parent.gameObject, holdTime, casterHold);
        }
        else
        {
            // 애니메이션만 출력
        }
    }
}