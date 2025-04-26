# FinalYearPro
This is my GitHub space for my final year project.

## What is it
This project is a reinforcement learning system that was built inside Unity using the ML-Agents toolkit. The agent is trained to act as a hider in a hide-and-seek environment. The goal is to gain important knowledge about this topic and to train an agent to hide intelligently from a seeker by using its environment and what it has learnet from reinforment lerning techniques.

## Tech used 
- Unity 2022.3 LTS
- ML-Agents Toolkit 0.30.0
- Python 3.10
- PyTorch
- TensorBoard
- NVIDIA RTX GPU - Used to accelerate training
  
## How to run
 1. **Set up a Python Virtual Environment (Recommended):**
 <br> Open a terminal where you want to create the virtual environment and run:
  ```bash
    python -m venv venv
  ```
 2. **Activate the Virtual Environment:**  
   Open a terminal in the same folder and run:
   - **Windows**:
     ```bash
       .\venv\Scripts\activate
     ```
   - **Mac/Linux**:
     ```bash
     source venv/bin/activate
     ```
 3. **Install the Required Python Packages:**
<br> Install ML-Agents and other dependencies:
 ```bash
  pip install mlagents torch tensorboard
  ```
 4. **Open Unity Project:**
<br> Open the Unity project using this repo inside Unity Hub and to be safe use the same version (Unity 2022.3 LTS)

 5. **Start a Training Session:**
 <br> Launch training from your terminal:
  ```bash
   mlagents-learn config/trainer-config.yaml --run-id=YourRunID --train
   ```
 6. **Play or Observe Agent Behavior:**
 <br> After training, you can run inference mode to watch the agent inside Unity.

## User manual 
