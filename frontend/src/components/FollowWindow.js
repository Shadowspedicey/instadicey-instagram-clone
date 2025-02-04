import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useSelector } from "react-redux";
import FollowButton from "./FollowButton";
import VerifiedTick from "./VerifiedTick";

const FollowWindow = props =>
{
	const { title, uids, closeFollowListWindow, newMessage } = props;
	const currentUser = useSelector(state => state.currentUser);
	const [list, setList] = useState(null);
	const [isLoading, setIsLoading] = useState(true);

	const getRoomID = async (targetUid) => 
	{
		// TODO: Gets chat room ID of the current user and a target user
	};

	const handleStartChat = async (targetUid) =>
	{
		closeFollowListWindow();
		// TODO: Create a chat room in the database for a first-time chat
	};

	useEffect(() =>
	{
		const getUidsInfo = async () =>
		{
			// TODO: Get following list as an array of users and set it to the list
			setIsLoading(false);
		};
		getUidsInfo();
	}, [uids]);

	return(
		<div className="backdrop container" onClick={closeFollowListWindow}>
			<div className="follow-list-window" onClick={e => e.stopPropagation()}>
				<div className="header">
					<h1>{title}</h1>
					<span className="close" onClick={closeFollowListWindow}>X</span>
				</div>
				{ isLoading
					? 
					<ul className="loading">
						{
							[0,1,2,3,4,5,6,7,8].map(n =>
								<li className="person" key={n}>
									<div className="profile">
										<div className="profile-pic"></div>
										<div className="info">
											<span className="real-name"></span>
											<span className="username"></span>
										</div>
									</div>
								</li>
							)
						}
					</ul>
					:
					<ul>
						{
							newMessage
								?
								list.map(person => 
									<Link to={`/direct/t/${getRoomID(person.uid)}`} className="person" key={person.uid} onClick={() => handleStartChat(person.uid, getRoomID(person.uid))}>
										<div className="profile">
											<div className="profile-pic"><img src={person.profilePic} alt={`${person.username}'s Pic`}></img></div>
											<div className="info">
												<div style={{display: "flex"}}><span className="username">{person.username}</span></div>
												<span className="real-name">{person.realName}</span>
											</div>
										</div>
									</Link>
								)
								:
								list.map(person => 
									<li className="person" key={person.uid}>
										<div className="profile">
											<Link to={`/${person.username}`}><div className="profile-pic"><img src={person.profilePic} alt={`${person.username}'s Pic`}></img></div></Link>
											<div className="info">
												<div style={{display: "flex"}}><Link to={`/${person.username}`} className="username">{person.username}</Link> <VerifiedTick user={person} size={15} marginLeft={7.5}/></div>
												<span className="real-name">{person.realName}</span>
											</div>
										</div>
										{currentUser && currentUser.user.uid === person.uid ? null : <FollowButton target={person} startLoading={() => setIsLoading(true)} stopLoading={() => setIsLoading(false)}/>}
									</li>
								)
						}
					</ul>
				}
			</div>
		</div>
	);
};

export default FollowWindow;
