This was my project for my Bachelor's Degree in Computer Science.
The focus of the dissertation was MAPF (Multi Agent Path Finding), and implementing some popular MAPF algorithms into the Unity game engine.
The algorithms I implemented were:
- A*, as a baseline, single-agent algorithm.
- STA* (Space-Time A*), an A* variant that also searches in the time dimension, so certain nodes can be blocked at specific time intervals.
- Cooperative A*, a variant of A* which can plan paths for multiple agents that avoids conflicts, by reserving positions in a space-time table.
- RRA* (Reverse Resumable A*), a variant of A* that operates backwards, and can be used in conjunction with other search algorithms to provide a more accurate heuristic.
- HCA* (Hierarchical Cooperative A*), a variant of Cooperative A* which uses RRA* as a heuristic.
- CBS (Conflict Based Search), a search based algorithm which is guaranteed to find the lowest cost solution to a MAPF problem, whilst creating conflict free paths.

Overall, the dissertation was moderately successful, as I succeeded in implementing a small selection of MAPF algorithms and my results were sound.
My thoughts on the project now are as such:
- The code I created for the project, whilst it mostly functioned as intended, was not very optimised and could be improved in many ways. Now I am a more competent programmer, the flaws in my program are more apparent.I will not chastise my past self too much however, as time was definitely a limiting factor during the project, and the amount of material to assist me in the project was relatively low, as MAPF is a somewhat niche field, especially in regards to videogame implementation.
- If I was to do the project again from scratch, I would spend less time trying to find material to help and more time attempting to implement the algorithms myself, as in the end, this was the method that allowed me to succeed.
- MAPF is an interesting field, but the applications in video games are relatively smal and niche, and heavily dependant on the genre of game.
