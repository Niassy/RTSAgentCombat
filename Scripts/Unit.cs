using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    // Health of the unit
    //[HideInInspector]
    public int m_health;

    // Health of the unit
    public int m_maxHealth;

    public int m_damage;

    public float m_weaponCooldown;

    [HideInInspector]
    public float m_weaponCurrentCooldown;

    [HideInInspector]
    public float m_weaponPreviousCooldown;

    [HideInInspector]
    public bool m_isInRangeShooting;

    // Fire transform for projectile
    public Transform m_fireTransform;

    public GameObject m_ProjectilePrefab;


    // Attack animation time
    // To prevent attacking one frame and moving in the same frame time
    // This will be linked to variable below
    public float m_attactTimeAnim;

    [HideInInspector]
    public float m_currentAttactTimeAnim;

    [HideInInspector]
    public bool m_canMove;

    // a reference to target ennemy
    public GameObject m_target;

    bool drawRay = false;

    RayPerception rayPer;

    [HideInInspector]
    public bool m_isAttacking;

    [HideInInspector]
    public bool m_timeAnimAttackFinish = false;

    [HideInInspector]
    // true if unit has attacked his target
    // when his cool down was 0
    // When it it is set true it means that cooldown >0
    // so if cool down reach 0 it is automoaticaaly set to false
    public bool m_hasAttacked;


    [HideInInspector]
    public int m_numProjectile;  // projectile spawn since attacking

    [HideInInspector]
    public int m_currentProjectile;  // num projecil


    // True if is going to attack target
    [HideInInspector]
    public bool m_isMovingForAttackTarget;

    // For movement
    public float m_speed;
    Vector3 m_destination;

    [HideInInspector]
    public bool m_isMoving;

    // reference to agent combat
    [HideInInspector]
    public AgentCombat m_agentCombatAI;

    // For our scenario is true this unit will seek to a target
    public float m_rangeShooting;

    private void Awake()
    {
        rayPer = GetComponent<RayPerception>();
    }

    // Use this for initialization
    void Start ()
    {
        m_destination = transform.position;
        m_health = m_maxHealth;
        m_numProjectile = 0;
        m_currentProjectile = 0;
        m_hasAttacked = false;
        m_isAttacking = false;
        m_isMoving = false;
        m_isMovingForAttackTarget = false;
        m_timeAnimAttackFinish = false;
	}
	
	// Update is called once per frame
	void Update ()
    {

        if (m_target && m_target.activeSelf)
        {
            Debug.Log("I am moving to my target");
            transform.position = Vector3.MoveTowards(transform.position, m_target.transform.position, m_speed * Time.deltaTime);
        }
        /*if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (drawRay == false)
            {
                drawRay = true;

                Debug.Log("Drawing ray");
                // draw a ray 
                // Use ray perception

                float rayDistance = 6f;
                //float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };


                float[] rayAngles = new float[361];

                for (int i = 0; i < 361; i++)
                    rayAngles[i] = i;

                string[] detectableObjects;
                detectableObjects = new string[] { "unit" };
                rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0.0f, 0f);
            }
        }*/

        // Update cool down attack
        UpdateWeaponCoolDown();
        /*if (m_weaponCurrentCooldown > 0)
        {
            m_weaponCurrentCooldown -= Time.deltaTime;

            if (m_weaponCurrentCooldown <= 0 )
            {
                m_weaponCurrentCooldown = 0;

                if (m_hasAttacked == true)
                    m_hasAttacked = false;
            }
        }*/

        if (m_isAttacking)
        {
            // update time for attacking
            m_currentAttactTimeAnim -= Time.deltaTime;

            if( m_currentAttactTimeAnim <=0 )
            {
                m_currentAttactTimeAnim = m_attactTimeAnim;
                m_isAttacking = false;
                m_canMove = true;
            }
        }

        //Debug.Log("Attacked = " + m_hasAttacked);
        /*if (m_hasAttacked == false)
        {
            if (m_currentProjectile < m_numProjectile)
            {
                Attack();
            }
        }*/

        // 
        if (m_isMovingForAttackTarget)
        {
            if (Vector3.Distance(m_destination, transform.position) <= m_rangeShooting)
            {
                m_canMove = false;
                m_isMoving = false;

                // attack
                if ( m_weaponCurrentCooldown <=0  /*!m_hasAttacked*/)
                {
                    Attack();
                    m_isMovingForAttackTarget = false;
                }
            }
        }

        if (m_canMove  && m_isMoving)
        {
            //Debug.Log("... I am moving to "+m_destination);
            Move(m_destination);
        }


        if (m_isMoving)
        {
            float distance = Vector3.Distance(m_destination, transform.position);
            Debug.Log("distance  = " + distance+" dest = "+m_destination);
            if (distance <= 0.01f)
            {
                m_isMoving = false;
            }
        }
    }

    void Attack()
    {
        //if (m_weaponCurrentCooldown <= 0)
        //{
        Debug.Log("Attacking target...");
        m_weaponCurrentCooldown = m_weaponCooldown;
        m_isAttacking = true;
        m_canMove = false;
        m_hasAttacked = true;


        // Spawn a projectile
        GameObject projectileObject = Instantiate(m_ProjectilePrefab, m_fireTransform.position, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();

        // projectile.m_shooter = this;
        // 28/04/2018
        projectile.m_shooter = this.gameObject;

        projectile.m_target = this.m_target;

        m_currentProjectile++;
        //}
    }

    public void AttackTarget(GameObject target)
    {
        if (target == null)
            return;

        //m_numProjectile++;
        if (!IsInRangeShooting(target))
        {
            Vector3 diff = target.transform.position - transform.position;
            m_destination = target.transform.position;
            m_canMove = true;
            m_isMoving = true;
            m_isMovingForAttackTarget = true;
            m_numProjectile++;
            //return;
        }

        else if (m_weaponCurrentCooldown <= 0)
        {
        
            m_weaponCurrentCooldown = m_weaponCooldown;
            m_isAttacking = true;
            m_canMove = false;
            m_hasAttacked = true;

            // Spawn a projectile
            GameObject projectileObject = Instantiate(m_ProjectilePrefab, m_fireTransform.position,Quaternion.identity);
            Projectile projectile = projectileObject.GetComponent<Projectile>();

            //projectile.m_shooter = this;
            // 28/04/2018
            projectile.m_shooter = this.gameObject;
            projectile.m_target = this.m_target;

            m_numProjectile++;
            m_currentProjectile++;
            Debug.Log(" projectile to deploy = "+m_numProjectile+" current projectile = "+m_currentProjectile);
        }

        else
        {
            m_numProjectile++;
        }
    }

    void UpdateWeaponCoolDown()
    {
        m_weaponCurrentCooldown -= Time.deltaTime;

        if (m_weaponCurrentCooldown <= 0)
        {
            m_weaponCurrentCooldown = 0;
        }
       
    }

    bool CanAttack(GameObject target)
    {
        if (m_weaponCooldown <=0
            ||  Vector3.Distance(target.transform.position,transform.position) <= m_rangeShooting)
        {
            return true;
        }

        return false;
    }

    bool IsInRangeShooting(GameObject target)
    {
        if (target == null)
            return false;

        float approxim = 0.01f;
        if (Vector3.Distance(target.transform.position, transform.position) <= m_rangeShooting + approxim)
        {
            return true;
        }

        return false;
    }

    bool HasAttackedTarget()
    {
        if (m_currentProjectile == m_numProjectile)
            return true;

        return false;
    }

    // Move to destionation
    private void Move(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(this.transform.position, destination, Time.deltaTime * m_speed);
    }

    void RotateFacingTarget(Vector3 destination)
    {
        transform.LookAt(destination);
    }

    #region SteeringBehaviour

    public void Flee(Vector3 destination,float fleeForce)
    {

        Vector3 diff = destination - transform.position;
        diff = diff.normalized;
        diff *= fleeForce;

        //Debug.Log(" diff = " + diff);
        Vector3 posToMove =  transform.TransformDirection(diff);

        m_destination =  transform.position -  diff;
        //m_destination = posToMove;
        //m_destination = destination;
    }

    void Evade()
    {

    }

    void Hide()
    {

    }


    #endregion

    #region MathUtils


    #endregion

    #region HealthComponent

    public void ReduceHealth(int amount)
    {
        m_health -= amount;
        if (m_health <=0)
        {
            //Destroy(gameObject);

            gameObject.SetActive(false);

        }
    }
    #endregion

}
