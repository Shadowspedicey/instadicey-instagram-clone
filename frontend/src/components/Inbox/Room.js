import { useEffect, useRef, useState } from "react";
import { useHistory } from "react-router";
import { Link } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import * as signalR from "@microsoft/signalr";
import Message from "./Message";
import { backend } from "../../config";
import { logOut } from "../../helpers";
import { setSnackbar } from "../../state/actions/snackbar";

const Room = ({roomID}) =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const messageRef = useRef();
	const [roomInfo, setRoomInfo] = useState(null);
	const [messages, setMessages] = useState(null);
	const [isLoading, setIsLoading] = useState(false);
	const [connection, setConnection] = useState(null);

	const getRoomInfo = async () => {
		try {
			const result = await fetch(`${backend}/chat/room?roomID=${roomID}`, {
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail);
			setRoomInfo(resultJSON);
		} catch (err) {
			dispatch(setSnackbar(err.message, "error"));
		}
	};

	const getMessages = async () =>
	{
		try {
			const result = await fetch(`${backend}/chat/messages?roomID=${roomID}`, {
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail);
			setMessages(resultJSON);
		} catch (err) {
			dispatch(setSnackbar(err.message, "error"));
		}
	};

	const sendMessage = async e =>
	{
		e.preventDefault();
		try
		{
			setIsLoading(true);
			const message = messageRef.current.value;
			if (message.trim() === "") throw new Error("Message is empty");

			await connection.invoke("SendMessage", message);

			messageRef.current.value = "";
		} catch (err) {
			console.error(err);
		}
		setIsLoading(false);
	};

	useEffect(() => document.title = "Instadicey • Chats");
	useEffect(() =>
	{
		const signalRConnection = new signalR.HubConnectionBuilder().withUrl(`${backend}/chat-hub?roomID=${roomID}`, { accessTokenFactory: () => localStorage.token }).withAutomaticReconnect().build();
		signalRConnection.start().then(() => {
			setConnection(signalRConnection);
			signalRConnection.on("ReceiveMessage", msg => setMessages(prev => [...prev, msg]));
		}).catch(err => {
			if (err.statusCode === 401)
				return logOut(dispatch, history);
		});

		return () => signalRConnection.stop();
	}, [roomID, dispatch, history]);

	useEffect(() =>
	{
		const getData = async () =>
		{
			await getRoomInfo();
			await getMessages();
		};
		getData();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [roomID]);

	if (!messages || !connection) return null;
	return(
		<div className="chat-room">
			<header><Link to={`/${roomInfo.users[0].username}`} style={{display: "flex", alignItems: "center"}}><span className="profile-pic" style={{marginRight: "1rem"}}><img src={roomInfo.users[0].profilePic} alt={`${roomInfo.users[0].username}'s pic`}></img></span>{roomInfo.users[0].username}</Link></header>
			<div className="messages">
				<div className="container">
					{
						messages.map((message, i, messagesArray) => 
							message.user.username === currentUser.username 
								? <Message messageInfo={message} noPhoto isSent key={message.id}/> 
								: messagesArray[i + 1] 
									? messagesArray[i].user.username === messagesArray[i + 1].user.username
										? <Message messageInfo={message} noPhoto key={message.id}/>
										: <Message messageInfo={message} key={message.id}/>
									: <Message messageInfo={message} key={message.id}/>
						)
					}
				</div>
			</div>
			<div className="send-message-container">
				<form className="outlined round" onSubmit={sendMessage}>
					<input className={`${isLoading ? "disabled" : ""}`} placeholder="Message..." ref={messageRef}/>
					<button className="text">Send</button>
				</form>
			</div>
		</div>
	);
};

export default Room;
