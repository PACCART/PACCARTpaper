# PACCART: Reinforcing Trust in Multiuser Privacy Agreement Systems
_**Daan Di Scala, Pinar Yolum**_


### Links
ArXiV paper: https://arxiv.org/abs/2302.13650 <br>
AAMAS23 paper: TBA <br>
COINE23 paper: https://coin-workshop.github.io/coine-2023-london/Papers/Paper-2.pdf <br>
Conference poster: https://raw.githubusercontent.com/PACCART/PACCARTpaper/master/AAMAS%20Poster%20Picture.png



This repository is part of our paper *"PACCART: Reinforcing Trust in Multiuser Privacy Agreement Systems".*

### Abstract
Collaborative systems, such as Online Social Networks and the Internet of Things, enable users to share privacy sensitive content. Content in these systems is often co-owned by multiple users with different privacy expectations, leading to possible multiuser privacy conflicts. In order to resolve these conflicts, various agreement mechanisms have been designed and agents that could participate in such mechanisms have been proposed. However, research shows that users hesitate to use software tools for managing their privacy. To remedy this, we argue that users should be supported by trustworthy agents that adhere to the following criteria: (i) concealment of privacy preferences, such that only necessary information is shared with others, (ii) equity of treatment, such that different kinds of users are supported equally, (iii) collaboration of users, such that a group of users can support each other in agreement and (iv) explainability of actions, such that users know why certain information about them was shared to reach a decision. Accordingly, this paper proposes PACCART, an open-source agent that satisfies these criteria. Our experiments over simulations and user study indicate that PACCART increases user trust significantly.  

### Running Example

![Running Example.](/fig0.png "Running Example.")


Consider the following running example: Alice has a car which is fitted with a security camera and Bob runs past it while wearing a smartwatch. The camera and smartwatch are both equipped with digital personal assistants that represent their users’ desires about privacy issues. Alice’s agent is instructed to record any, possibly suspicious, behavior, while Bob does not want to be recorded. The agents have a dispute by providing arguments on whether or not to record. Eventually, Bob’s agent convinces Alice’s agent that he is not suspicious and thus the camera does not record any movement. 

The dispute is as follows: Alice’s agent starts with an argument in favor of the subject: ‘record bob, for security reasons, as he might be a thief’. Bob’s agent extends the dispute with adding two counterarguments: 1) ‘Bob is not a thief, as he regularly jogs by, based on doctor's orders' and 2) ‘Bob is not a thief, as he is very rich'. Alice’s agent has no counterarguments, so the dispute (as shown in Figure 1 below) ends. Because Bob's agent wins the dispute, Bob is **not** recorded by Alice’s agent. Based on the outcome of the dispute, a privacy related action is autonomously taken.

![Example Dispute.](/fig1.png "This is an Example Dispute.")

*Figure 1. Example Dispute. Attack relations between arguments are red lines. Supportive relations within arguments are black lines.*

### Code
We provide our agent, experimental setup and dispute dataset generator as open-source programs in order to reproduce the results reported in our paper and for future work.

## Requirements
The PACCART agent is implemented in a Unity 2021.1.9f1 environment with C# 8.0.

## Running the Code
The agent environment can be ran through the Unity editor. `DisputeRenderer.cs` is the main file which starts the visualization of the example dispute as shown above in Figure 1. 

## Classes

An overview of the structure of the PACCART classes is given in Figure 2.

![PACCART Classes.](/fig2.png "PACCART Classes.")

*Figure 2. PACCART Classes.*

The system holds the necessary classes such as an Agent class, Dispute class, and a Knowledge Base class. It also contains classes for Rules, Premises and Contraries. The entire system is controlled from an Environment class, which initializes a dispute between multiple agents and allows them to extend the dispute in turns. Handcrafted scenarios which are used for additional validation for our research are stored in a Scenarios class. Finally, for each of the PACCART components (called modules in the implementation, see Modules class) experiments are simulated, which are controlled through the Experiments class and evaluated through the Metrics class. Data is stored and fetched through serialization, which has classes for both JSON objects and CSV files.

### User Study
For the user study that we conducted we created a questionnaire consisting of two parts. The first part consists of questions on the privacy stances of users and the second part consists of questions about the perceived trust of users on the system. Part 2 is introduced with a running example (and picture) and an explanation of the system workings. In the interview study this points serves as a checkpoint, where participants are asked whether they fully understand the purpose of the system. The information on the user study can be found in the “User Study.pdf” file.
