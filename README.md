# FinalYearPro
This is my GitHub space for my final year project.
<br> Screencast:https://youtu.be/eP5jBk0jpvE
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

 7. **Using TensorBoard:**
 <br> After or during training, you can use the following command to view your results on multiple graphs in real time:
 ```bash
 tensorboard --logdir results
 ```
## User manual

1. **Training the Agent:**

- Open a terminal, activate your virtual environment.
- Run the following command to begin training:

  ```bash
  mlagents-learn config/trainer-config.yaml --run-id=YourRunID --train 
  ```
  **--train** can be **--resume** to continue training or **--force** to start fresh
  <br> Inside Unity, click the **Play** icon to allow the agent to interact with the environment
  
 2. **Watching the Agent:**
    - After training, click on the **Play** button in Unity to watch the trained agent interact with the environment and show off it's new behavior
      
>**Note:**
> If you would like to manually test a specific trained model (e.g. a **.nn** file you have from previous training), you can: 
> - Add the **.nn** file into your Unity Project (Into an Assets folder).
> - Select the Agent GameObject in your scene.
> - In the **Behavior Parameters** component:
>   - Assign the imported model under the **Model** field.
>   - Set the **Behavior Type** to **Inference Only**.
> - Press **Play** to watch the agent behave based on the model selected.
> - If you would like to use one of  the models I trained. I added two too "ML-Agents-Models", "hiding-walls-v3" is the final working model.
