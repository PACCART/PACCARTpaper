# PACCART: Reinforcing Trust in Multiuser Privacy Agreement Systems

This implementation is part of our paper *"PACCART: Reinforcing Trust in Multiuser Privacy Agreement Systems".*

### Abstract
Collaborative systems, such as Online Social Networks and the Internet of Things, enable users to share privacy sensitive content. Content in these systems is often co-owned by multiple users with different privacy expectations, leading to possible multiuser privacy conflicts. In order to resolve these conflicts, various agreement mechanisms have been designed and agents that could participate in such mechanisms have been proposed. However, research shows that users hesitate to use software tools for managing their privacy. To remedy this, we argue that users should be supported by trustworthy agents that adhere to the following criteria: (i) concealment of privacy preferences, such that only necessary information is shared with others, (ii) equity of treatment, such that different kinds of users are supported equally, (iii) collaboration of users, such that a group of users can support each other in agreement and (iv) explainability of actions, such that users know why certain information about them was shared to reach a decision. Accordingly, this paper proposes PACCART, an open-source agent that satisfies these criteria. Our experiments over simulations and user study indicate that PACCART increases user trust significantly.  

### Code
We provide our agent, experimental setup and dispute dataset generator as open-source programs in order to reproduce the results reported in our paper and for future work.

## Requirements
The PACCART agent is implemented in a Unity 2021.1.9f1 environment with C# 8.0.

## Running the Code
The agent environment can be ran through the Unity editor. `DisputeRenderer.cs` is the main file which starts the visualisation of the example dispute as shown below in Figure 1. 

![Example Dispute.](/fig1.png "This is an Example Dispute.")

*Figure 1. Example Dispute with one argument in favor of the subject (e.g., record a passerby for security reasons as they might be a thief) and two counterarguments (e.g., 'the passerby is no  thief as they regularly jog by based on doctor's orders' or 'the passerby is no thief as they are very rich')* 

## Classes

An overview of the structure of the PACCART classes is given in Figure 2.

![PACCART Classes.](/fig2.png "PACCART Classes.")

*Figure 2. PACCART Classes.*

The system holds the necessary classes such as an Agent class, Dispute class, and a Knowledge Base class. It also contains classes for Rules, Premises and Contraries. The entire system is controlled from an Environment class, which initializes a dispute between multiple agents and allows them to extend the dispute in turns. Handcrafted scenarios which are used for additional validation for our research are stored in a Scenarios class. Finally, for each of the PACCART components (called modules in the implementation, see Modules class) experiments are simulated, which are controlled through the Experiments class and evaluated through the Metrics class. Data is stored and fetched through serialization, which has classes for both JSON objects and CSV files.
