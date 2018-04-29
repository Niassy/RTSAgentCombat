using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Agent combat model for RTS games combat

public class AgentCombat : Agent
{

    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    public GameObject area;

    /// <summary>
    /// The area bounds.
    /// </summary>
	[HideInInspector]
    public Bounds areaBounds;

    PushBlockAcademy academy;

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    //public GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    //public GameObject block;

    /// <summary>
    /// Detects when the block touches the goal.
    /// </summary>
	[HideInInspector]
    public GoalDetect goalDetect;

    //Rigidbody blockRB;  //cached on initialization
    Rigidbody agentRB;  //cached on initialization
    Material groundMaterial; //cached on Awake()
    RayPerception rayPer;

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer groundRenderer;

    // reference to unit component
    Unit m_unit;

    // for caluclating reward based on previous state and current state
    int m_enemyHealthPrevState;

    int m_healthPrevState;

    // how much the agent move on a direction
    float m_steeringForce;

    // Keeping track of next action and previous action
    int m_nextAction;

    // The previous action is action that is going to be executed or
    // already exectuted by agent while next action is an action which
    // agent wish to process
    // The next action change at each frame
    int m_previousAction;


    void Awake()
    {
        // There is one brain in the scene so this should find our brain.
        brain = FindObjectOfType<Brain>();
        academy = FindObjectOfType<PushBlockAcademy>(); //cache the academy

        m_unit = GetComponent<Unit>();
        m_unit.m_agentCombatAI = this;
    }

    private void Start()
    {
        m_nextAction = -1;
        m_previousAction = -1;
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        //goalDetect = block.GetComponent<GoalDetect>();
        //goalDetect.agent = this;
        rayPer = GetComponent<RayPerception>();

        // Cache the agent rigidbody
        agentRB = GetComponent<Rigidbody>();
        // Cache the block rigidbody
       // blockRB = block.GetComponent<Rigidbody>();

        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        groundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        groundMaterial = groundRenderer.material;
    }

    public override void CollectObservations()
    {
        /* float rayDistance = 12f;
         float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
         string[] detectableObjects;
         detectableObjects = new string[] { "block", "goal", "wall" };
         AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
         AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 1.5f, 0f));*/


        /*AddVectorObs(m_unit.m_weaponCurrentCooldown / m_unit.m_weaponCooldown);
        AddVectorObs(m_unit.m_health / m_unit.m_maxHealth);

        int hasAttacked = 0;
        if (m_unit.m_hasAttacked)
            hasAttacked = 1;

        float dist = Vector3.Distance(m_unit.transform.position, transform.position);

        AddVectorObs(hasAttacked);
        AddVectorObs(dist/12f);*/
        AddVectorObs(m_steeringForce);
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        bool foundNewSpawnLocation = false;
        Vector3 randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            float randomPosX = Random.Range(-areaBounds.extents.x * academy.spawnAreaMarginMultiplier,
                                areaBounds.extents.x * academy.spawnAreaMarginMultiplier);

            float randomPosZ = Random.Range(-areaBounds.extents.z * academy.spawnAreaMarginMultiplier,
                                            areaBounds.extents.z * academy.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.01f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void IScoredAGoal()
    {
        // We use a reward of 5.
        AddReward(5f);

        // By marking an agent as done AgentReset() will be called automatically.
        Done();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(academy.goalScoredMaterial, 0.5f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        groundRenderer.material = groundMaterial;
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
	public void MoveAgent(float[] act)
    {

        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        Debug.Log("Action = " + act[0]);
        int action = Mathf.FloorToInt(act[0]);

        // Goalies and Strikers have slightly different action spaces.
        switch (action)
        {
            case 0:
                dirToGo = transform.forward * 1f;
                break;
            case 1:
                dirToGo = transform.forward * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
            case 3:
                rotateDir = transform.up * -1f;
                break;
            case 4:
                dirToGo = transform.right * -0.75f;
                break;
            case 5:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agentRB.AddForce(dirToGo * academy.agentRunSpeed,
                         ForceMode.VelocityChange);

    }

    /// <summary>
    ///  Called every step of the engine. Here the agent takes an action.
    ///  In our context of RTS combat,agent can two types of action (Retreat or Fight)
    ///  Retreat meand agent move to a position in order to hide from enemy
    ///  Fight means agent attack target
    ///  This work is inpired from the articles : #Applying Reinforcement Learning to Small Scale Combat in the Real-Time Strategy Game StarCraft:Broodwar#
    ///  To make thing simple, when agent wants to retreat, we will compute a vector that will escape from the target
    ///  if agent want to fight(he will wait until his cool down is beyond 0
    /// // if agent wants to retreat and he was previously attacking,he has to wait until the attack animation frames animations ends
    ///  Be delighted by the code
    /// </summary>
	public override void AgentAction(float[] vectorAction, string textAction)
    {
        // We will use discrete action
        // 0 means agent attack
        // 1 means agant move forward

        // In case of discrete actions
        int action = Mathf.FloorToInt(vectorAction[0]);

       // Debug.Log("action = " + action);

        // If unit is already performing an action
        // do not allow to take another ac
       /* if (m_unit.m_isAttacking || m_unit.m_isMoving
            || m_unit.m_isMovingForAttackTarget)
            return;*/

        Debug.Log("Processing action = " + action);

        // Next action to be processed is more priority to execued that the
        // current action

        if (action == -1)
        {
            action = Random.Range(0, 2);
            //action = 1;
            //action = 0;
        }

        Debug.Log("Random action = " + action);

        bool agentIsProcessingAction = false;

        // Use next action or previous action to make management
        if (m_previousAction == 0) // agent was attacking 
        {
            if (m_nextAction == 0)   // He want also to attack next time he will be able(weapon cooldown)
            {
                Debug.Log("Requesting an attack for next action");
                if (/*m_unit.m_hasAttacked*/
                   m_unit.m_weaponCurrentCooldown <=0)   // Okay unit can attack
                {
                    Debug.Log("Next Action is processed");
                    agentIsProcessingAction = true;
                    m_unit.AttackTarget(m_unit.m_target);
                    m_previousAction = m_nextAction;
                }
            }

            else if (m_nextAction == 1)
            {
                m_steeringForce = 1.0f;
                m_unit.Flee(m_unit.m_target.transform.position, m_steeringForce);
                m_unit.m_isMoving = true;
                m_unit.m_canMove = true;
                m_previousAction = m_nextAction;
            }

            else
            {
                //Debug.Log("Agent was attacking...");
                if (m_unit.m_currentProjectile == m_unit.m_numProjectile)   // Okay unit can attack
                {
                    Debug.Log("cur proj = " + m_unit.m_currentProjectile + " num proj = " + m_unit.m_numProjectile);
                    Debug.Log("Agent was attacking and as finished...");
                    agentIsProcessingAction = true;
                    m_unit.AttackTarget(m_unit.m_target);
                    //m_nextAction = 0;
                }
            }
        }

        else if  (m_previousAction == 1) // Agent was moving
        {
            if (m_unit.m_isMoving == false) // He has finished move
            {
                if (m_nextAction == 0)
                {
                    agentIsProcessingAction = true;
                    m_unit.AttackTarget(m_unit.m_target);
                    m_previousAction = m_nextAction;
                }

                else if (m_nextAction == 1)
                {

                    Debug.Log("Processig next action move");
                    m_steeringForce = 1.0f;
                    m_unit.Flee(m_unit.m_target.transform.position, m_steeringForce);
                    m_unit.m_isMoving = true;
                    m_unit.m_canMove = true;
                    m_previousAction = action;
                }

            }
        }
     
        switch (action)
        {
            case 0:  // Want to attack
                bool attackOrMoveToAttack = false;
               
                if (m_previousAction == 0)  // If the previous action was an attack
                {
                    Debug.Log(" Receiving attack action Previous attack was an attack ... "+ " Current projectile = "
                        + m_unit.m_currentProjectile + " num proj = " + m_unit.m_numProjectile);
                    if (/*m_unit.m_hasAttacked */
                        m_unit.m_currentProjectile == m_unit.m_numProjectile)
                    {
                        attackOrMoveToAttack = true;
                        //canRegisterNextAction = true;
                        Debug.Log("Current projectile = "+m_unit.m_currentProjectile+" num proj = "+m_unit.m_numProjectile);
                    }

                    else  // The unit has not yet attacked and he has choosed to attack next time cool down is on
                    {
                        m_nextAction = action;
                    }
                }

                else if (m_previousAction == 1)  // That means the agent was moving
                {
                    if (m_unit.m_isMoving == false)
                    {
                        attackOrMoveToAttack = true;
                    }
                    else
                    {
                        m_nextAction = action;
                    }
                }

                else  // Default action
                {
                    attackOrMoveToAttack = true;
                    Debug.Log("Previous action is Default action = so attacking... ");
                }
                
                if (attackOrMoveToAttack)
                {
                    Debug.Log("Attacking target  previous = " + m_previousAction);
                    m_previousAction = action;
                    m_unit.AttackTarget(m_unit.m_target);
                }
               
                //m_unit.AttackTarget(m_unit.m_target);
                break;

            case 1:  // Want to retreat

                //Debug.Log("MOving action");
                bool move = false;

                if (m_previousAction == 0)  // If the previous action was an attack
                {
                    if (m_unit.m_hasAttacked)
                    {
                        //move = true;

                        //canRegisterNextAction = true;
                    }

                    else  // The unit has not yet attacked and he has choosed to attack next time cool down is on
                    {
                        m_nextAction = action;
                    }
                }

                else if (m_previousAction == 1)  // That means the agent was moving
                {
                    Debug.Log("Previous action is moving");
                    if (m_unit.m_isMoving == false)
                    {
                        Debug.Log(" Unit is not moving ");
                        move= true;
                    }

                    else
                    {
                        Debug.Log("Next action is also a move ...");
                        m_nextAction = action;
                    }
                }

                else
                {
                    move = true;
                    Debug.Log("New action move is created...");
                }

                if (move)
                {
                    Debug.Log("Moving action");

                    if (m_unit.m_target != null)
                    {

                        m_previousAction = action;
                        m_steeringForce = 1.0f;
                        m_unit.Flee(m_unit.m_target.transform.position, m_steeringForce);
                        m_unit.m_isMoving = true;
                        m_unit.m_canMove = true;
                        Debug.Log("... I am retreating...");
                    }
                }

               /* m_steeringForce = 4.0f;
                m_unit.Flee(m_unit.m_target.transform.position, m_steeringForce);
                m_unit.m_isMoving = true;
                m_unit.m_canMove = true;
                Debug.Log("... I am retreating...");*/
                break;
        }
        
        // We will use simple

        // If agent choose to move 

        // Move the agent using the action.
       // MoveAgent(vectorAction);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / agentParameters.maxStep);
    }

    void AgentAttack()
    {
        if (m_unit.m_weaponCurrentCooldown <=0)
        {
            // Okay he can attack
        }
    }

    void AgentRetreat()
    {
    }

    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlock()
    {
        // Get a random position for the block.
       // block.transform.position = GetRandomSpawnPos();

        // Reset block velocity back to zero.
        //  blockRB.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        //blockRB.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be 
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
	public override void AgentReset()
    {
        int rotation = Random.Range(0, 4);
        float rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        ResetBlock();
        //transform.position = GetRandomSpawnPos();
        agentRB.velocity = Vector3.zero;
        agentRB.angularVelocity = Vector3.zero;

        m_nextAction = -1;
        m_previousAction = -1;

    }

    // Get unit component
    Unit GetUnit()
    {
        return m_unit;
    }

    // Inpired by article : 
    // #Applying Reinforcement Learning to Small Scale Combat in the Real-Time Strategy Game StarCraft:Broodwar#
    private void ComputeReward()
    {
        // get both current health of enemy and agent
        int hpEnemy = m_unit.m_target.GetComponent<Unit>().m_health;
        int myHp = m_unit.m_health;
    }

    public void IKilledTarget()
    {
        AddReward(1);
    }

    public void IWasKilled()
    {
    }
}
