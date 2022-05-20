FROM linuxserver/ffmpeg

RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install mysql-client python-dev python-pip -y && \
    apt-get clean

RUN pip install awscli

ENTRYPOINT echo "Grabbing file" && \
    aws s3 cp ${FROM_S3_FILE} input.mp4 && \
    echo "Transcoding file" && \
    ffmpeg -i input.mp4 -vf scale=480:-1 -preset veryslow -tune zerolatency output.mp4 && \
    echo "Saving file" && \
    aws s3 cp output.mp4 ${TO_S3_FILE} && \
    echo "Updating record" && \
    mysql -u${DATABASE_USER} -p${DATABASE_PASSWORD} -h${DATABASE_SERVER} -D${DATABASE_NAME} -e"UPDATE videos SET is_processing = 0 WHERE id = '${ID}';"