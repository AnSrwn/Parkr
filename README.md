# Parkr
Parkr is a simulation in which a reinforcement learning agent learns how to park a car.
In order to do this, Unity ML-Agents is used.

## Setup 
* Unity Version: 2019.3.15f1
* ML Agents Package Version: 1.0.2
* Python Version: 3.7.7

## Demo
For the demo a 10 min trained model is included.

1. Open the project in Unity.
1. Select and open the scene 'ParkingLot' in the folder 'Scenes'.
1. Click Play.

## Training

### Strategies and Learnings
| Strategy  | Outcome   | Learning  |
| --------- | --------- | --------- |
| Positive rewards for reaching target and negative rewards for collisions. | Agent generates too much negative rewards through collisions, so that it learns to leave the map. | First train without negative rewards for collision. |
| Constantly giving a negative reward (time penalty), so that the agent reaches the target faster and more directly. | Agent learns to leave the map as fast as possible to reduce negative rewards through time penalty. | First train without time penalty. |
| Giving rewards for intermediate goals, so that the agent learns to move in the direction of the target. It was tried with every 5 meters, 1 meter or for each action. The closer the agent got to the target, the more reward it received. | Agent learns to drive in circles, because it generates rewards. | Could be a solution if the intermediate goals can only be reached once. |
| Only giving rewards for reaching the target and using curiosity module (different curiosity strengths were tested). | No training improvement was observed. | Maybe it shows an effect if training continues a few days longer. |
| Reducing complexity: Start simple and build up. First stage: Agent needs to reach the target without any obstacles. Second stage: Include obstacles. Third stage: Negative reward for collisions with obstacles. Fourth stage: Time penalty | Agent could not even complete the first stage. | Seems logically and probably works, but a lot of time for training is needed. |
| Increase number of hidden layers. Changed num_layer from 2 to 3. | Learning processes seemed to improve. | If the problem is complex, more hidden layers are needed. |
| Simplify observations: The relative position to the target is maybe not clear enough. So, the new observations are the absolute locations of the agent and the target. | No training improvement was observed. | Keep the observations simple is probably always good. But if absolute locations are better than relatives, could not be confirmed. |

### Start Training
1. Change Behavior Type of Behavior Parameters of the Car to *Default*.
1. Create two folders with the name `models` and `summaries` in the root folder.
1. Open the unity project folder in a CLI.
1. Execute the following commands:
```bash
mlagents-learn
mlagents-learn ./ParkrCar.yaml --run-id ParkrCar --train
```

### Using Tensorboard
1. Open the unity project folder in a CLI.
2. Execute the following command:
```bash
tensorboard --logdir=summaries --port=6006
```
3. Open http://localhost:6006

## Observations
There are 14 observations.
* Current speed of the car (1)
* Absolute postion of the car (2)
* Absolute position of the parking spot (2)
* Distance to close objects through 8 proximity sensors (8)
* Angle relative to parking spot (1)

## Actions
There are 2 actions.
* Speed
* Steering

## Rewards
* Collision with other objects: -0.01 (every time actions are received)
* Time penalty: -0.01 (every time actions are received)
* Reached parking spot: +1
  * Coming to a stop: +1
  * Parked parallel to markings: +1
