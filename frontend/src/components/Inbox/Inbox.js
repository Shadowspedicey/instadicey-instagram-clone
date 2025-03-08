import { useEffect, useState } from "react";
import { Route, useParams } from "react-router";
import { NavLink, Switch } from "react-router-dom";
import { useMediaQuery } from "react-responsive";
import { useDispatch, useSelector } from "react-redux";
import { formatDistanceToNowStrict } from "date-fns";
import * as signalR from "@microsoft/signalr";
import Room from "./Room";
import FollowWindow from "../FollowWindow";
import SendMessage from "../../assets/misc/send-message.png";
import "./inbox.css";
import { backend } from "../../config";
import { logOut } from "../../helpers";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";
import { setSnackbar } from "../../state/actions/snackbar";
import { startLoading, stopLoading } from "../../state/actions/isLoading";

const Inbox = () =>
{
	const { roomID } = useParams();
	const currentUser = useSelector(state => state.currentUser);
	const dispatch = useDispatch();
	const history = useHistory();
	const [currentUserFollows, setCurrentUserFollows] = useState(null);
	const [recentChats, setRecentChats] = useState(null);
	const [isNewMessageBoxOpen, setIsNewMessageBoxOpen] = useState(false);
	const closeNewMessageBox = () => setIsNewMessageBoxOpen(false);
	const phoneQuery = useMediaQuery({query: "(max-width: 600px)"});

	useEffect(() => document.title = "Inbox • Chats");

	const formatDate = date =>
	{
		let dateStringArray = date.split(" ");
		dateStringArray[1] = dateStringArray[1].slice(0, 1);
		const dateString = dateStringArray.join(" ");
		return dateString;
	};

	const getChats = async () =>
	{
		if (!currentUser) return;
		try {
			const result = await fetch(`${backend}/chat/rooms`, {
				headers: {
					Authorization: `Bearer ${localStorage.token}`
				}
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail);

			setRecentChats(resultJSON);
		} catch(err) {
			console.error(err);
		}
	};

	const getUsersFollows = async () =>
	{
		if (!currentUser) return;
		try {
			const result = await fetch(`${backend}/user/${currentUser.username}`);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });
			setCurrentUserFollows({following: resultJSON.following, followers: resultJSON.followers});
		} catch (err) {
			dispatch(setSnackbar(err.message));
		}
	};
	
	useEffect(() => {
		if (!currentUser) return;
		const getData = async () => {
			dispatch(startLoading());
			await getUsersFollows();
			await getChats();
			dispatch(stopLoading());
		};
		getData();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [currentUser]);

	useEffect(() => {
		if (!currentUser) return;
		const connection = new signalR.HubConnectionBuilder().withUrl(`${backend}/chat-hub`, { accessTokenFactory: () => localStorage.token }).withAutomaticReconnect().build();
		connection.start().then(() => {
			connection.on("UpdateRoom", chatRoom => {
				var found = false;
				chatRoom.lastUpdated = chatRoom.lastUpdated.slice(0, -1);
				chatRoom.users = chatRoom.users.filter(u => u.username !== currentUser.username);
				setRecentChats(prev => prev.map(c => {
					if (c.id === chatRoom.id) {
						found = true;
						return chatRoom;
					} else return c;
				}));
				if (!found)
					setRecentChats(prev => [...prev, chatRoom]);
			});
			return () => connection.stop();
		}).catch(err => {
			if (err.statusCode === 401)
				return logOut(dispatch, history);
		});

		return () => connection.stop();
	}, [currentUser, dispatch, history]);

	if (!currentUser || !recentChats) return null;
	if (phoneQuery)
		return(
			<div className="inbox-window outlined">
				{ isNewMessageBoxOpen &&
						<FollowWindow title="New Message" users={[...currentUserFollows.following, ...currentUserFollows.followers]} closeFollowListWindow={closeNewMessageBox} newMessage/>
				}
				<div className="container">
					<Switch>
						<Route exact path="/direct/inbox">
							<div className="recent-chats left">
								<header>{currentUser.username}<button className="icon" onClick={() => setIsNewMessageBoxOpen(true)}><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fillRule="evenodd" clipRule="evenodd"><path d="M8.071 21.586l-7.071 1.414 1.414-7.071 14.929-14.929 5.657 5.657-14.929 14.929zm-.493-.921l-4.243-4.243-1.06 5.303 5.303-1.06zm9.765-18.251l-13.3 13.301 4.242 4.242 13.301-13.3-4.243-4.243z"/></svg></button></header>
								<ul>
									{
										recentChats.sort((a, b) => new Date(b.lastUpdated) - new Date(a.lastUpdated)).map(chat =>
											<NavLink to={`/direct/t/${chat.id}`} activeClassName="selected" key={chat.id}>
												<li key={chat.id}>
													<span className="profile-pic"><img src={chat.users[0].profilePic} alt={`${chat.users[0].username}'s profile pic`}></img></span>
													<div className="info">
														<span className="username">{chat.users[0].username}</span>
														<div className="message">
															<div>{chat.lastMessage?.message}</div>
															{chat.lastUpdated && <span>• {formatDate(formatDistanceToNowStrict(new Date(chat.lastUpdated+"Z")))}</span>}
														</div>
													</div>
												</li>
											</NavLink>
										)
									}
								</ul>
							</div>
						</Route>
						<Route path="/direct/t/:roomID">
							<Room roomID={roomID}/>
						</Route>
					</Switch>
				</div>
			</div>
		);
	else return(
		<div className="inbox-window outlined">
			{ isNewMessageBoxOpen &&
					<FollowWindow title="New Message" users={[...currentUserFollows.following, ...currentUserFollows.followers]} closeFollowListWindow={closeNewMessageBox} newMessage/>
			}
			<div className="container">
				<div className="recent-chats left">
					<header>{currentUser.username}<button className="icon" onClick={() => setIsNewMessageBoxOpen(true)}><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fillRule="evenodd" clipRule="evenodd"><path d="M8.071 21.586l-7.071 1.414 1.414-7.071 14.929-14.929 5.657 5.657-14.929 14.929zm-.493-.921l-4.243-4.243-1.06 5.303 5.303-1.06zm9.765-18.251l-13.3 13.301 4.242 4.242 13.301-13.3-4.243-4.243z"/></svg></button></header>
					<ul>
						{
							recentChats.sort((a, b) => new Date(b.lastUpdated) - new Date(a.lastUpdated)).map(chat =>
								<NavLink to={`/direct/t/${chat.id}`} activeClassName="selected" key={chat.id}>
									<li key={chat.id}>
										<span className="profile-pic"><img src={chat.users[0].profilePic} alt={`${chat.users[0].username}'s profile pic`}></img></span>
										<div className="info">
											<span className="username">{chat.users[0].username}</span>
											<div className="message">
												<div>{chat.lastMessage?.message}</div>
												{chat.lastUpdated && <span>• {formatDate(formatDistanceToNowStrict(new Date(chat.lastUpdated+"Z")))}</span>}
											</div>
										</div>
									</li>
								</NavLink>
							)
						}
					</ul>
				</div>
				<div className="chat-window right">
					<Switch>
						<Route exact path="/direct/inbox">
							<div style={{height: "100%", display: "flex", flexDirection: "column", justifyContent: "center", alignItems: "center", gap: "0.75rem"}}>
								<div style={{height: 175}}><img src={SendMessage} alt="Send a message"/></div>
								<span style={{fontWeight: 500, fontSize: "1.25em"}}>Your Messages</span>
								<span style={{color: "#8e8e8e", fontSize: "0.95em"}}>Send private photos and messages to a friend or group.</span>
								<button style={{width: "initial"}} onClick={() => setIsNewMessageBoxOpen(true)}>Send Message</button>
							</div>
						</Route>
						<Route path="/direct/t/:roomID"><Room roomID={roomID}/></Route>
					</Switch>
				</div>
			</div>
		</div>
	);
};

export default Inbox;
