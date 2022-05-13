FROM jrottenberg/ffmpeg

RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install python-dev python-pip -y && \
    apt-get clean

RUN pip install awscli

ENTRYPOINT aws s3 cp ${FROM_S3_FILE} input.mp4 && \
    ffmpeg -i input.mp4 -vf scale=480:-1 -preset veryslow -tune zerolatency output.mp4 && \
    aws s3 cp output.mp4 ${TO_S3_FILE}