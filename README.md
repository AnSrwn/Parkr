# Parkr
Parkr is a simulation in which a reinforcement learning agent learns how to park a car.
In order to do this, Unity ML-Agents is used.

## Setup 
* Unity Version: 2019.3.15f1
* ML Agents Package Version: 1.0.2
* Python Version: 3.7.7

### Demo
For the demo a 10 min trained model is included.

1. Open the project in Unity.
1. Select and open the scene 'ParkingLot' in the folder 'Scenes'.
1. Click Play.

### Training
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
There are 11 observations.
* Current speed of the car (1)
* Car position relative to parking spot (2)
* Distance to close objects through 8 proximity sensors (8)

## Actions
There are 2 actions.
* Speed
* Steering

## Rewards
* Collision with other objects: -0.1 (every time actions are received)
* Getting closer to parking spot: +0.1 (every 5 meters)
* Getting farther away from parking spot: -0.1 (every 5 meters)
* Reached parking spot: +1
  * Parked parallel to markings: +0.5
  * Time needed: between 0 and +1 (secondsRemaining/maxEpisodeLength)
