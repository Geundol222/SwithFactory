using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviourPun
{
    public bool onGizmos;
    public float range;
    [Range(0, 360)] public float angle;

    protected Canvas skillCanvas;
    protected PhotonView parentView;
    protected GameObject target;
    protected LayerMask playerMask;
    protected Vector3 boxSize;

    protected float cosResult;
    protected Vector3 targetDir;

    protected virtual void Awake()
    {
        skillCanvas = GetComponentInChildren<Canvas>();
        playerMask = LayerMask.GetMask("SkillPoint");
        boxSize = new Vector3(1f, 2f, 0.5f);

        cosResult = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
    }

    protected virtual void Start()
    {
        parentView = transform.parent.gameObject.GetPhotonView();

        if (!photonView.IsMine)
        {
            skillCanvas.gameObject.SetActive(false);
        }
    }

    public virtual void ActiveSkill()
    {
        // 플레이어 본인의 캐릭터 이면,
        if (photonView.IsMine)
        {
            // Overlap을 사용하여 playerMask에 해당하는 LayerMask를 지닌 모든 객체의 콜라이더 수집
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, playerMask);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetPhotonView().IsMine)
                    continue;

                // 타겟의 방향벡터 정규화
                targetDir = (collider.transform.position - transform.position).normalized;  

                // 내적을 계산해서 범위안에 들어왔다면 Target 지정
                if (Vector3.Dot(transform.forward, targetDir) > cosResult)  
                {
                    // Skill 객체를 가진 플레이어를 지정해야하므로 parent
                    GetTarget(collider.transform.parent.gameObject);    
                }
            }

            if (target != null)
                print($"SetTarget, ViewID : {target.GetPhotonView().ViewID}");
        }
    }

    private void GetTarget(GameObject newObj)
    {
        // 타겟이 없으면 새로운 객체를 초기 타겟으로 지정
        if (target == null)
            target = newObj;
        else
        {
            // 기존 타겟보다 가까운 객체를 타겟으로 설정 foreach문에 의해 가장 가까운 타겟이 지정됨
            if ((target.transform.position - transform.position).sqrMagnitude > (newObj.transform.position - transform.position).sqrMagnitude)
                target = newObj;
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (onGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);

            Vector3 rightDir = AngleToDir(transform.eulerAngles.y + angle * 0.5f);
            Vector3 leftDir = AngleToDir(transform.eulerAngles.y - angle * 0.5f);
            Debug.DrawRay(transform.position, rightDir * range, Color.red);
            Debug.DrawRay(transform.position, leftDir * range, Color.red);
        }
    }

    private Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }
}
