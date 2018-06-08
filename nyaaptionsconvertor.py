# simple tool to convert the old nyaaoptions format to the new one.

import json
from pprint import pprint
import io

with open('nyaaoptions.json') as f:
	data = json.load(f)

print(len(data["SubscribedAnime"]))
	
for su in data["SubscribedAnime"]:
	s = su["Anime"]["Subgroup"]
	su["Anime"]["Subgroup"] = [s, "Erai-raws"]
	su["Anime"]["MinQuality"] = "x720";
	print("hi")

file = open("nyaaoptions.json", "w")
file.write(json.dumps(data, indent=4))
file.close()