using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    // Reference to agent who shoot
    [HideInInspector]
    //public Unit m_shooter;
    public GameObject m_shooter;

    [HideInInspector]
    public GameObject m_target;

    public float m_speed;

    public int m_damageShooter;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
	    // Move to target
        if (m_target)
        {
            transform.position = Vector3.MoveTowards(this.transform.position, m_target.transform.position, Time.deltaTime * m_speed);

            if (Vector3.Distance(transform.position,m_target.transform.position) < 1.2f)
            {
                //m_target.GetComponent<Unit>().ReduceHealth( m_shooter.m_damage);
                m_target.GetComponent<Unit>().ReduceHealth(m_damageShooter);

                Destroy(gameObject);
            }

        }
	}

    void SeekTo(Vector3 destination)
    {

    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("I am colliding ...");
        // Touched goal.
        if (col.gameObject.CompareTag("unit")
            && col.gameObject == m_target)
        {
            Debug.Log("I am beign destroyed");
            //  Notfy other other agent or unit
            //col.gameObject.GetComponent<Unit>().m_health -= m_shooter.m_damage;

            col.gameObject.GetComponent<Unit>().m_health -= m_damageShooter;

            Destroy(gameObject);
        }
    }
}
