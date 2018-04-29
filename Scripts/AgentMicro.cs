using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMicro : Agent{

    #region Attack

    [Header("Attack")]

    public float m_weaponCD;
    float m_currentWeaponCD;
    public int m_damage;
    public float m_rangeWeapon;

    // Attack animation time
    // To prevent attacking one frame and moving in the same frame time
    // This will be linked to variable below
    public float m_attactTimeAnim;

    [HideInInspector]
    public float m_currentAttactTimeAnim;

    // These variables are used for determine if an unit has
    // attacked when an attack action was requested
    // An unit has attacked if the number of projectile to deploy

    int m_numProjectileToDeploy;  // 
    int m_numDeployedProjectile;  // num projeci

    // Fire transform for projectile
    public Transform m_fireTransform;
    public GameObject m_ProjectilePrefab;

    #endregion

    #region Movement

    [Header("Movement")]
    public float m_speed;

    Vector3 m_destination;

    #endregion

    #region TargetingSystem

    [Header("TargetingSystem")]
    public GameObject m_target;

    #endregion

    #region SteeringBehaviour


    public float m_fleeForce;

    #endregion

    #region Action

    float m_currentAction;
    float m_nextAction;
    float m_previousAction;

    #endregion

    #region MachineLearning

    public float m_timeEpisode;
    float m_currentTimeEpisode;

    
    Vector3 m_startingPos;
    Vector3 m_TargetStartingPos;

    public float m_maxDistancefromTarget;

    // There variables above must be normalises
    public float m_lastWeaponCooldown;
    public float m_lastTargetHealth;  // Target health before action is taken
    public float m_lastTargetDistance;  // target distance before action is taken

    public float m_lastCurrentAction;
    public float m_lastNextAction;

    #endregion

    // Use this for initialization
    void Start ()
    {
        m_currentWeaponCD = 0;
        m_currentAttactTimeAnim = 0;

        m_numDeployedProjectile = 0;
        m_numDeployedProjectile = 0;

        m_currentAction = -1;
        m_nextAction = -1;
        m_currentTimeEpisode = 0;

        m_startingPos = transform.position;
        m_TargetStartingPos = m_target.transform.position;

        m_lastWeaponCooldown = -1;  // 
        m_lastTargetDistance = -1;
        m_lastWeaponCooldown = -1;

    }

	// Update is called once per frame
	void Update ()
    {

        if (m_target && m_target.activeSelf == true)
        {
            Vector3 pos1 = transform.position;
            Vector3 pos2 = m_target.transform.position;

            pos1.y = 0;
            pos2.y = 0;

            float dist = Vector3.Distance(pos1, pos2);

            Debug.Log("dist = " + dist);
            if (dist <= 1.7f)
            {
                Debug.Log("I am dead");
                Done();
                //gameObject.SetActive(false);
            }
        }
		// Update weapon cooldown
        if (m_currentWeaponCD >0)
        {
            m_currentWeaponCD -= Time.deltaTime;

            if (m_currentWeaponCD <= 0)
                m_currentWeaponCD = 0;
        }

        if (m_currentAttactTimeAnim >0)
        {
            //Debug.Log("Time anim attack " + m_currentAttactTimeAnim);
            m_currentAttactTimeAnim -= Time.deltaTime;

            if (m_currentAttactTimeAnim <= 0)
                m_currentAttactTimeAnim = 0;
        }

        m_currentTimeEpisode += Time.deltaTime;

        if (m_currentTimeEpisode >= m_timeEpisode)
        {
            Done();
            m_currentTimeEpisode = 0;
        }
	}

    #region MachineLearning

    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    public override void AgentReset()
    {
        m_currentWeaponCD = 0;
        m_currentAttactTimeAnim = 0;

        m_numDeployedProjectile = 0;
        m_numDeployedProjectile = 0;

        m_currentAction = -1;
        m_nextAction = -1;
        m_currentTimeEpisode = 0;

        transform.position = m_startingPos;

        m_target.transform.position = m_TargetStartingPos;

        m_target.SetActive(true);
        m_target.GetComponent<Unit>().m_health = m_target.GetComponent<Unit>().m_maxHealth;

        m_lastWeaponCooldown = -1;  // 
        m_lastTargetDistance = -1;
        m_lastWeaponCooldown = -1;


    }


    public override void CollectObservations()
    {
        // We must know the actions(current,and next action)
        // Also the properties agent ie health,damage,weapon_cd
        // distance from target,etc...

        // What we expect from a good learnig is that
        // when agent weapon cooldown <0 it attack target
        // after he flee to safe position
        // But if the target is too close when agent has to attack
        // he can also flee

        AddVectorObs(m_currentAction);
        AddVectorObs(m_nextAction);
        AddVectorObs(m_previousAction);
        //AddVectorObs(m_currentWeaponCD);

        // Now getting information from target
        float dist = Vector3.Distance(transform.position, m_target.transform.position);
       // Unit target = m_target.GetComponent<Unit>();

        //AddVectorObs(target.m_health);
        //AddVectorObs(dist);

        // We only add observation of these parameters
        // from the moment when action is set
        // The action can be current or next
        AddVectorObs(m_lastWeaponCooldown);
        AddVectorObs(m_lastTargetDistance);
        AddVectorObs(m_lastTargetHealth);

        if (m_lastWeaponCooldown != -1)
        {
            m_lastWeaponCooldown = -1;
        }

        if (m_lastTargetDistance != -1)
        {
            m_lastTargetDistance = -1;
        }

        if (m_lastTargetHealth!= -1)
        {
            m_lastTargetHealth = -1;
        }
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);

        // For testing only
        //action = 1; // Attack action

       action = Random.Range(0, 2);

        Debug.Log("current action = " + action);

        if (NoCurrentActionExist())  // It means that there was no action
        {
            if (action != -1)
            {

                m_lastWeaponCooldown =  m_currentWeaponCD / m_weaponCD;
                m_lastTargetHealth = m_target.GetComponent<Unit>().m_health / m_target.GetComponent<Unit>().m_maxHealth ;
                m_lastTargetDistance = Vector3.Distance(transform.position,m_target.transform.position) / m_maxDistancefromTarget;


                m_currentAction = action;
                //Debug.Log("New action set");

                if (m_currentAction == 0)
                {
                    m_numProjectileToDeploy++;

                    // Compute reward
                    ComputeAttackRewardForCurrentAction();

                }

                else if (m_currentAction == 1)
                {
                    // Compute flee
                    Flee(m_target.transform.position, m_fleeForce);

                    // Compute flee reward
                    ComputeFleeRewardForCurrentAction();
                }

                //ProcessCurrentAction();
            }
        }

        else
        {
           // Debug.Log("Current action exist");
            if (!NextActionExist())
            {
                // Debug.Log("Next action set");

                if (action != -1)
                {
                    m_nextAction = action;

                    m_lastWeaponCooldown = m_currentWeaponCD / m_weaponCD;
                    m_lastTargetHealth = m_target.GetComponent<Unit>().m_health / m_target.GetComponent<Unit>().m_maxHealth;
                    m_lastTargetDistance = Vector3.Distance(transform.position, m_target.transform.position) / m_maxDistancefromTarget;

                    if (m_nextAction == 0)  // Attack action chosed for next action
                    {
                        ComputeAttackRewardForNextAction();
                    }

                    else if (m_nextAction == 1)  // Flee action chosed for next action
                    {
                        ComputeFleeRewardForNextAction();
                    }

                }
            }

            //Debug.Log("current time anim " + m_currentAttactTimeAnim);
            if (IsCurrentActionFinished())
            {
                //Debug.Log("Action is Finished random action = "+action);
                m_previousAction = m_currentAction;
                m_currentAction = action;

                if (NextActionExist())
                {
                    m_currentAction = m_nextAction;

                    //Debug.Log("Processing new actions");

                    if (m_currentAction == 0)
                    {
                        //Debug.Log("New action attack  deploy = " + m_numProjectileToDeploy + " current = " + m_numDeployedProjectile);
                    }

                    // remove next action
                    m_nextAction = -1;
                }
  
                if (m_currentAction == 0)
                {
                    m_numProjectileToDeploy++;
                }

                else if (m_currentAction == 1)
                {
                    // compute flee
                    Flee(m_target.transform.position, m_fleeForce);
                }
                //ProcessCurrentAction();
            }
        }

        ProcessCurrentAction();

    }

    // Computing reward attack for the current action

     void ComputeAttackRewardForCurrentAction()
     {
        //  This is a basic implementation

        // The reward attack is proportional to the weapon cooldown
        //  weapon cooldown reward = 1 if weapon cooldown =0 
        //                 = 0.2 if weapon can not fire

        // Also we take also in account the distance
        //  distance reward = m_nornmalised_distance

        // health reward 
        // if the attack kill the target then
        // health reward = 0.5



        // So we call attack reward AR  
        // AR = weapon cooldown reward + distance reward + health reward + next action reward

        // AR must be computed when taking new action

        float dist = Vector3.Distance(transform.position, m_target.transform.position);

        // Get the target health
        if (m_currentWeaponCD > 0)
        {
            if (dist < 2)
                AddReward(-0.5f);

            else if (dist > 5)
                AddReward(0.5f);
        }

        else  // It means agent can attack target
        {
            // 
            // If target too close give him a good reward for having flee
            if (dist < 2)
            {
                AddReward(0.5f);
            }

            // Also penalize him when target is far 
            else if (dist > 5)
            {
                AddReward(-0.5f);
            }
        }
    }

    // This will be called when agent set next action
    void ComputeAttackRewardForNextAction()
    {
        // We will also reward the next action
        // next action reward = 

        // If current action is Attack

        // if target too cloose 
        // if flee choosed then reward = 0.2
        //  // if attack then reward = -0.2                  

        // if target not close
        // if flee reward = -0.2
        // if attack  reward = 0.2

        float dist = Vector3.Distance(transform.position, m_target.transform.position);

        // 
        if (m_currentAction == 0)  // Current action is an attack
        {
            if (dist < 2)
                AddReward(-0.5f);

            // Also penalize him when target is far 
            else if (dist > 5)
            {
                AddReward(0.5f);
            }
        }

        else if (m_currentAction == 1)   //Current action is Flee
        {
            // 
            AddReward(0.5f);
        }

    }

    void ComputeFleeRewardForCurrentAction()
    {
        float fleeReward = 0;
        float dist = Vector3.Distance(transform.position, m_target.transform.position);


        if (m_currentWeaponCD >0 )
        {
            AddReward(1.0f);
        }

        else  // It means agent can attack target but he has decided to flee
        {
            
            // 
            // If target too close give him a good reward for having flee
            if (dist < 2 )
            {
                AddReward(0.5f);
            }

            // Also penalize him when target is far 
            else if (dist > 5)
            {
                AddReward(-0.5f);
            }
        }

    }

    void ComputeFleeRewardForNextAction()
    {
        float dist = Vector3.Distance(transform.position, m_target.transform.position);

        // 
        if (m_currentAction == 0)  // Current action is an attack
        {
            if (dist < 2)
                AddReward(1.0f);

            // Also penalize him when target is far 
            else if (dist > 5)
            {
                AddReward(-0.5f);
            }

        }

        else if (m_currentAction == 1)   //Current action is Flee
        {
            // 
            // If target too close give him a good reward for having flee
            if (dist < 2)
            {
                AddReward(0.5f);
            }

            // Also penalize him when target is far 
            else if (dist > 5)
            {
                AddReward(-0.5f);
            }
        }
    }
   
    #endregion

    #region Actions

    void ProcessCurrentAction()
    {
        if (m_currentAction == 0)  // Attack or Attack Move
        {
            //Debug.Log("Attacking target");
            if (!HasAttackedTarget(m_target))
               AttackTarget(m_target);
        }

        else if (m_currentAction == 1)  // Retreat
        {
            transform.position = Vector3.MoveTowards(transform.position, m_destination, m_speed * Time.deltaTime);
        }
    }

    // Return true if currrent action is  exexcuted
    // and is not finished
    bool IsCurrentActionBeingExecuted()  
    {
        if (m_currentAction == 0)
        {
            if (!HasAttackedTarget(m_target))
                return true;
        }
        return false;
    }

    bool IsCurrentActionNotAlreadyExecuted()
    {
        if (m_currentAction == 0) // Attack
        {
            if (!HasAttackedTarget(m_target))
                return true;
        }
        return false;
    }

    bool IsCurrentActionFinished()
    {
       // Debug.Log("IsCurrentActionFinished :: ");
        if (m_currentAction == 0)
        {
           // Debug.Log("IsCurrentActionFinished :: action = Attack");
            //Debug.Log("IsCurrentActionFinished :: numProjToDeploy = " + m_numProjectileToDeploy + " numDeployedProjectile = " + m_numDeployedProjectile);
            if (HasAttackedTarget(m_target))
            {
                //Debug.Log("I have attacked target :: numProjToDeploy = "+m_numProjectileToDeploy+" numDeployedProjectile = "+m_numDeployedProjectile);
                //Debug.Log("current time anim attack = " + m_currentAttactTimeAnim);
                if (TimeAnimAttackFinish())
                {
                    //Debug.Log("Time anim is finished");
                    return true;
                }

                else
                {
                   // Debug.Log("Time anim not finished");
                }
            }
        }

        else if (m_currentAction == 1)
        {

            float dist = Vector3.Distance(m_destination, transform.position);
            //Debug.Log("distance = " + dist+" destination = "+m_destination);
            if (HasReachedDestination(0.09f))
            {
                Debug.Log("I have reached destination = "+m_destination );
                return true;
            }
        }

        return false;
    }

    bool NoCurrentActionExist()
    {
        return m_currentAction == -1;
    }

    bool NextActionExist()
    {
        return m_nextAction != -1;
    }

    #endregion

    #region Attack

    void AttackTarget(GameObject target)
    {
        if (CanAttackTarget(target))
        {

            Debug.Log("I can Attack Target...");
            // When cooldown was active or when agnet was not in range
            /*if (m_numProjectileToDeploy > m_numDeployedProjectile)
            {
                m_numDeployedProjectile++;
            }

            else  // When agent is range et cool down < 0
            // but was not attacking 
            {
                m_numProjectileToDeploy++;
                m_numDeployedProjectile++;
            }*/

            m_numDeployedProjectile++;


            // Spawn a projectile
            SpawnProjectile(this.gameObject, m_target, m_damage);

            m_currentWeaponCD = m_weaponCD;
            m_currentAttactTimeAnim = m_attactTimeAnim;
        }

        else
        {
            /*if (m_numProjectileToDeploy == m_numDeployedProjectile)
            {
                m_numProjectileToDeploy++;
            }*/
        }
    }

    void SpawnProjectile(GameObject shooter,GameObject target,int damage)
    {
        // Spawn a projectile
        GameObject projectileObject = Instantiate(m_ProjectilePrefab, m_fireTransform.position, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        // projectile.m_shooter = this;
        // 28/04/2018
        projectile.m_shooter = shooter;
        projectile.m_target = target;
        projectile.m_damageShooter = damage;
    }

    bool CanAttackTarget(GameObject target)
    {
        if (target == null  || target.activeSelf == false )
            return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);
        if (dist <= m_rangeWeapon)
        {
            if (m_currentWeaponCD <= 0)
                return true;
        }

        return false;
    }

    bool HasAttackedTarget(GameObject target)
    {
        if (m_numProjectileToDeploy == 0)
            return false;

       if (m_numDeployedProjectile == m_numProjectileToDeploy)
           return true;

        return false;
    }

    bool TimeAnimAttackFinish()
    {
        if (m_currentAttactTimeAnim <=0)
        {
            return true;
        }
        return false;
    }

    #endregion


    #region Movement

    void MoveTo(Vector3 destination)
    {
        m_destination = destination;
        transform.position = Vector3.MoveTowards(transform.position, destination, m_speed * Time.deltaTime);
    }

    bool HasReachedDestination(float distanceAcceptance = 0.0f)
    {
        float distance = Vector3.Distance(transform.position, m_destination);
        return (distance <= distanceAcceptance);
    }

    #endregion

    #region SteeringBehaviour

    public void Flee(Vector3 destination, float fleeForce)
    {
        if (m_target == null  || m_target.activeSelf == false)
            return;
        Vector3 diff = destination - transform.position;
        diff.y = 0;

        diff = diff.normalized;


        //Debug.Log("diff  = " + diff);
        diff *= fleeForce;

        //Debug.Log("Flee force = " + diff.z);
        Vector3 posToMove = transform.TransformDirection(diff);
        m_destination = transform.position - diff;

        m_destination.y = transform.position.y;
    }

    void Evade()
    {

    }

    void Hide()
    {

    }

    #endregion

    #region Collision

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("I am colliding ...");
        // Touched goal.
        if (col.gameObject.CompareTag("unit")
            && col.gameObject == m_target)
        {
           // Done();
        }
    }
    #endregion
}
