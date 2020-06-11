# Parkr
Parkr is a simulation in which a reinforcement learning agent learns how to park a car. In order to do this, Unity ML-Agents is used.

## Setup and start training:
* Unity Version: 2019.3.15f1
* ML Agents Package Version: 1.0.2
* Python Version: 3.7.7

1. Change Behavior Type of Behavior Parameters of the Car to 'Default'   
1. Create two folders with the name 'models' and 'summaries' in the root folder
1. Open PowerShell (or similar console) in root folder
1. 'mlagents-learn'
1. 'mlagents-learn ./ParkrCar.yaml --run-id ParkrCar --train'
