import { useEffect, useRef, useState } from "react";
import { useHistory } from "react-router";
import { Link } from "react-router-dom";
import { useSelector } from "react-redux";
import { nanoid } from "nanoid";
import Message from "./Message";

const Room = ({roomID}) =>
{
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const messageRef = useRef();
	const [otherUser, setOtherUser] = useState(null);
	const [messages, setMessages] = useState(null);
	const [isLoading, setIsLoading] = useState(false);

	const getOtherUserInfo = async () =>
	{
		try
		{
			// TODO: Get the other user's information and set it and return it
		} catch (err)
		{
			console.error(err);
			if (err.message === "Room doesn't exist")
				history.push("/direct/inbox");
		}
	};

	const getMessages = async () =>
	{
		try
		{
			// TODO: Get all the messages in the room as an array and set it
		} catch (err)
		{
			console.error(err);
		}
	};

	const sendMessage = async e =>
	{
		e.preventDefault();
		try
		{
			setIsLoading(true);
			const message = messageRef.current.value;
			const id = nanoid(32);
			if (message.trim() === "") throw new Error("Message is empty");

			// TODO: Send message
			// await Promise.all(
			// 	[
			// 		setDoc(doc(db, "chats", roomID, "messages", id),
			// 			{
			// 				id,
			// 				message,
			// 				user: currentUser.user.uid,
			// 				timestamp: serverTimestamp(),
			// 			}),
			// 		updateDoc(doc(db, "chats", roomID),
			// 			{
			// 				lastMessage: {id, message},
			// 				lastUpdated: serverTimestamp(),
			// 			})
			// 	]
			// );

			messageRef.current.value = "";
			setIsLoading(false);
		} catch (err)
		{
			console.error(err);
		}
	};

	useEffect(() => document.title = "Instadicey • Chats");
	useEffect(() =>
	{
		// TODO: RSignal stuff and WebSockets
		// const q = query(collection(db, "chats", roomID, "messages"), orderBy("timestamp", "asc"));
		// const unsubscribe = onSnapshot(q, querySnapshot => setMessages(querySnapshot.docs.map(doc => doc.data())));

		// return () => unsubscribe();
	}, [roomID]);

	useEffect(() =>
	{
		const getData = async () =>
		{
			if (getOtherUserInfo())
			{
				getMessages();
			}
		};
		getData();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [roomID]);

	if (!otherUser || !messages) return null;
	return(
		<div className="chat-room">
			<header><Link to={`/${otherUser.username}`} style={{display: "flex", alignItems: "center"}}><span className="profile-pic" style={{marginRight: "1rem"}}><img src={otherUser.profilePic} alt={`${otherUser.username}'s pic`}></img></span>{otherUser.username}</Link></header>
			<div className="messages">
				<div className="container">
					{
						messages.map((message, i, messagesArray) => 
							message.user === currentUser.user.uid 
								? <Message messageInfo={message} userInfo={currentUser.info} noPhoto isSent key={message.id}/> 
								: messagesArray[i + 1] 
									? messagesArray[i].user === messagesArray[i + 1].user
										? <Message messageInfo={message} userInfo={otherUser} noPhoto key={message.id}/>
										: <Message messageInfo={message} userInfo={otherUser} key={message.id}/>
									: <Message messageInfo={message} userInfo={otherUser} key={message.id}/>
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
