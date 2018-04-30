
This the documentation for our Real time Strategy MicroManagement for Machine Learning.
Work are in progress and currenly we have created a scenario in which there is agent who will try to kill a target.
The goal of the learning process is to have an agent able performing Kiting Behaviour.
An agent perfom=rm Kiting when he attack his target and flee to free position to avoid being hittec by his target. 

Here are the informations for the scenario we have created

* Setup  :A platforming environment when the agent can attack or flee a target
* Goal : The agent must kill the target
* Agents: The environment contains one link linked to a single brain
Actions : Two actions are possible for agent : Attacking target or Flee
 As in RTS games,actions are durative ,So an action must be executed(current action) and finish before performing next action(next action)

* Agent reward function : As in RTS games,actions are durative,we have decided to calculate reward when agent take an
 action.
     If agent choose to flee(next action) after attacking(current action) he receives good reward
     if agent choose to attack when target it is close,he will receive negative reward
     If agent choose to flee when target is too far,he will also receive negative reward.
