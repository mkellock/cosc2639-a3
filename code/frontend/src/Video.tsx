import { ArrowDownIcon, ArrowUpIcon } from '@chakra-ui/icons';
import { AspectRatio, Button, Center, HStack, Text, VStack } from '@chakra-ui/react';
import React from 'react';

interface InputProps {
    id: string;
    upvotes: number;
    downvotes: number;
    city: string;
    region: string;
    country: string;
    upvote(id: string): void;
    downvote(id: string): void;
}

export default class Video extends React.Component<InputProps> {
    state = {
        upvotes: this.props.upvotes,
        downvotes: this.props.downvotes,
    };

    render(): React.ReactNode {
        let imageUrl: string = '/compressed/' + this.props.id + '.mp4';

        return (
            <Center
                style={{
                    backgroundColor: 'Black',
                    borderRadius: '25px',
                    padding: '10px',
                    width: '400px',
                    height: '420px',
                }}
            >
                <VStack>
                    <AspectRatio width={370} height={370}>
                        <iframe src={imageUrl} allowFullScreen title="Video" />
                    </AspectRatio>
                    <HStack>
                        <Button
                            leftIcon={<ArrowUpIcon />}
                            size="xs"
                            onClick={() => {
                                this.setState({ upvotes: this.state.upvotes + 1 });
                                this.props.upvote(this.props.id);
                            }}
                        >
                            {this.state.upvotes}
                        </Button>
                        &nbsp;
                        <Button
                            leftIcon={<ArrowDownIcon />}
                            size="xs"
                            onClick={() => {
                                this.setState({ downvotes: this.state.downvotes + 1 });
                                this.props.downvote(this.props.id);
                            }}
                        >
                            {this.state.downvotes}
                        </Button>
                        <Text fontSize="xs">
                            {this.props.city} - {this.props.region} - {this.props.country}
                        </Text>
                    </HStack>
                </VStack>
            </Center>
        );
    }
}
