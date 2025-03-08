import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import FollowButton from "./FollowButton";
import VerifiedTick from "./VerifiedTick";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";
import { startLoading, stopLoading } from "../state/actions/isLoading";
import { backend } from "../config";
import { logOut } from "../helpers";
import { setSnackbar } from "../state/actions/snackbar";

const FollowWindow = props =>
{
	const { title, users, closeFollowListWindow, newMessage } = props;
	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const [isLoading, setIsLoading] = useState(false);

	const handleStartChat = async (targetUsername) =>
	{
		closeFollowListWindow();
		dispatch(startLoading());
		try {
			const result = await fetch(`${backend}/chat/room`, {
				method: "POST",
				headers: {
					Authorization: `Bearer ${localStorage.token}`,
					"Content-Type": "application/json"
				},
				body: JSON.stringify([targetUsername])
			});
			if (result.status === 401)
				return logOut(dispatch, history);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail);
			history.push(`/direct/t/${resultJSON.id}`);
		} catch (err) {
			dispatch(setSnackbar(err.message, "error"));
		}
		dispatch(stopLoading());
	};

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
								users.map(person => 
									<Link to="" className="person" key={person.username} onClick={() => handleStartChat(person.username)}>
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
								users.map(person => 
									<li className="person" key={person.username}>
										<div className="profile">
											<Link to={`/${person.username}`}><div className="profile-pic"><img src={person.profilePic} alt={`${person.username}'s Pic`}></img></div></Link>
											<div className="info">
												<div style={{display: "flex"}}><Link to={`/${person.username}`} className="username">{person.username}</Link> <VerifiedTick user={person} size={15} marginLeft={7.5}/></div>
												<span className="real-name">{person.realName}</span>
											</div>
										</div>
										{currentUser && currentUser.username === person.username ? null : <FollowButton target={person} startLoading={() => setIsLoading(true)} stopLoading={() => setIsLoading(false)}/>}
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
