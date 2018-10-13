import requests;
import re;
import json;
import pprint;
import time;
import sys;

def search(keywords, max_results=None):
    url = 'https://duckduckgo.com/';
    params = {
    	'q': keywords
    };

    #   First make a request to above URL, and parse out the 'vqd'
    #   This is a special token, which should be used in the subsequent request
    res = requests.post(url, data=params)
    searchObj = re.search(r'vqd=(\d+)\&', res.text, re.M|re.I);

    headers = {
    'dnt': '1',
    'accept-encoding': 'gzip, deflate, sdch, br',
    'x-requested-with': 'XMLHttpRequest',
    'accept-language': 'en-GB,en-US;q=0.8,en;q=0.6,ms;q=0.4',
    'user-agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36',
    'accept': 'application/json, text/javascript, */*; q=0.01',
    'referer': 'https://duckduckgo.com/',
    'authority': 'duckduckgo.com',
    }

    params = (
    ('l', 'wt-wt'),
    ('o', 'json'),
    ('q', keywords),
    ('vqd', searchObj.group(1)),
    ('f', ',,,'),
    ('p', '2'),
    ('iax', 'images'),
    ('ia', 'images')
    )

    requestUrl = url + "i.js";

    res = requests.get(requestUrl, headers=headers, params=params);    
    data = json.loads(res.text);
    printJson(data["results"]);
    if "next" not in data:
        exit(0);
    requestUrl = url + data["next"];
    time.sleep(5);

def printJson(objs):
    for obj in objs:
        print("{0}".format(obj["image"]))
        
a = sys.argv
a.pop(0)
s = " ".join(a)    
search(s)