#!/bin/bash
docker run \
	-v mongoData:/data/db \
	-p 27021:27017 \
	--name mongodb-xenia-revolt \
	-d \
	-e MONGO_INITDB_ROOT_USERNAME=user \
	-e MONGO_INITDB_ROOT_PASSWORD=password mongo 
