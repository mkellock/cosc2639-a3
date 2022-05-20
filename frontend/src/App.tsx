import * as React from 'react';
import { Button, ChakraProvider, HStack, IconButton, Input, Spacer, theme, Wrap, WrapItem } from '@chakra-ui/react';
import axios from 'axios';
import Video from './Video';
import { RepeatIcon } from '@chakra-ui/icons';
import { ColorModeSwitcher } from './ColorModeSwitcher';

export default class App extends React.Component {
    state = {
        file: '',
        videos: '',
    };

    componentDidMount() {
        this.refreshVideos();
    }

    refreshVideos = () => {
        axios
            .get('https://api.kellock.com.au/video/get_videos', {
                responseType: 'json',
                transformResponse: [(v) => v],
            })
            .then((response) => {
                this.setState({ videos: response.data });
            });
    }
    
    handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        let files = event.target.files;
        let reader = new FileReader();

        if (files !== null) {
            reader.readAsDataURL(files[0]);

            reader.onload = (e) => {
                if (e.target === null || e.target === undefined) {
                    this.setState({ file: '' });
                } else {
                    this.setState({ file: e.target.result });
                }
            };
        }
    };

    submitVideo = async () => {
        if (this.state.file.length > 5000000) {
            alert('File too big: Please upload a video file less than 5MB');
        } else if (this.state.file.length > 0) {
            const res = await axios.get('https://api.my-ip.io/ip');
            var bodyFormData = new FormData();
            bodyFormData.append('ip', res.data);
            bodyFormData.append('contents', this.state.file);

            axios({
                method: 'post',
                url: 'https://api.kellock.com.au/video/upload/',
                data: bodyFormData,
                headers: { 'Content-Type': 'multipart/form-data' },
            })
                .then(() => {
                    alert('File uploaded: Your file has been successfully uploded and will be available shortly');
                })
                .catch(() => {
                    alert('Upload error: There has been an upload error, please try another video file');
                });
        } else {
            alert('Please select a file: Please select a file before submitting');
        }
    };

    upvote = (id: string) => {
        axios({
            method: 'post',
            url: 'https://api.kellock.com.au/video/upvote/' + id,
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    };

    downvote = (id: string) => {
        axios({
            method: 'post',
            url: 'https://api.kellock.com.au/video/downvote/' + id,
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    };

    render(): React.ReactNode {
        return (
            <ChakraProvider theme={theme}>
                <div
                    style={{
                        width: '100vw',
                        height: '100vh',
                    }}
                >
                    <div
                        style={{
                            width: '100vw',
                            backgroundColor: 'Black',
                        }}
                    >
                        <HStack padding={5}>
                            <Input type="file" width="450px" onChange={(e) => this.handleFileChange(e)} accept="video/*" />
                            <Button onClick={() => this.submitVideo()}>Submit</Button>
                            <Spacer></Spacer>
                            <ColorModeSwitcher />
                            <IconButton aria-label="Reload" icon={<RepeatIcon />} onClick={this.refreshVideos} />
                        </HStack>
                    </div>
                    <Wrap padding={5}>
                        {this.state.videos.length > 0
                            ? JSON.parse(this.state.videos)?.map((videoItem: any) => (
                                  <WrapItem key={videoItem['id'] + 'I'}>
                                      <Video key={videoItem['id']} id={videoItem['id']} upvotes={videoItem['upvotes']} downvotes={videoItem['downvotes']} city={videoItem['city']} region={videoItem['region']} country={videoItem['country']} upvote={this.upvote} downvote={this.downvote}></Video>
                                  </WrapItem>
                              ))
                            : null}
                    </Wrap>
                </div>
            </ChakraProvider>
        );
    }
}
