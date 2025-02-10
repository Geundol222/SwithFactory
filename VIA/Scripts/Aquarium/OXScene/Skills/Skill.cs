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
        // �÷��̾� ������ ĳ���� �̸�,
        if (photonView.IsMine)
        {
            // Overlap�� ����Ͽ� playerMask�� �ش��ϴ� LayerMask�� ���� ��� ��ü�� �ݶ��̴� ����
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, playerMask);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetPhotonView().IsMine)
                    continue;

                // Ÿ���� ���⺤�� ����ȭ
                targetDir = (collider.transform.position - transform.position).normalized;  

                // ������ ����ؼ� �����ȿ� ���Դٸ� Target ����
                if (Vector3.Dot(transform.forward, targetDir) > cosResult)  
                {
                    // Skill ��ü�� ���� �÷��̾ �����ؾ��ϹǷ� parent
                    GetTarget(collider.transform.parent.gameObject);    
                }
            }

            if (target != null)
                print($"SetTarget, ViewID : {target.GetPhotonView().ViewID}");
        }
    }

    private void GetTarget(GameObject newObj)
    {
        // Ÿ���� ������ ���ο� ��ü�� �ʱ� Ÿ������ ����
        if (target == null)
            target = newObj;
        else
        {
            // ���� Ÿ�ٺ��� ����� ��ü�� Ÿ������ ���� foreach���� ���� ���� ����� Ÿ���� ������
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
