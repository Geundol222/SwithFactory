using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    public GameObject bloodParticle;
    ParticleSystem particleSystem;
    public List<GameObject> bloodList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleCollision(GameObject other)
    {
        List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        int numCollisionEvents = particleSystem.GetCollisionEvents(other , collisionEvents);
        print(other.name);
        for (int i = 0; i < numCollisionEvents; i++)
        {
            // �浹 ��ġ ��������
            Vector3 collisionPosition = collisionEvents[i].intersection;
            // ���� ������Ʈ ����
            bloodList.Add(Instantiate(bloodParticle , collisionPosition , Quaternion.identity));
        }
    }
}
