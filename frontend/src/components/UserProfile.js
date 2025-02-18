import { useEffect, useState } from "react";
import { Route, Switch, useParams } from "react-router";
import { Link, NavLink } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import { startLoading, stopLoading } from "../state/actions/isLoading";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min.js";
import { backend } from "../config.js";
import { logOut } from "../helpers.js";
import BrokenPage from "./BrokenPage";
import FollowButton from "./FollowButton";
import FollowWindow from "./FollowWindow";
import PostCard from "./Posts/PostCard";
import VerifiedTick from "./VerifiedTick";
import "../styles/user-profile.css";

const UserProfile = () =>
{
	const { username } = useParams();

	const dispatch = useDispatch();
	const history = useHistory();
	const currentUser = useSelector(state => state.currentUser);
	const [userData, setUserData] = useState(null);
	const [savedPosts, setSavedPosts] = useState(null);
	const [isFollowingListWindowOpen, setIsFollowingListWindowOpen] = useState(false);
	const [isFollowersListWindowOpen, setIsFollowersListWindowOpen] = useState(false);
	const closeFollowListWindow = () =>
	{
		setIsFollowingListWindowOpen(false);
		setIsFollowersListWindowOpen(false);
	};

	const getUserData = async () =>
	{
		try
		{
			const result = await fetch(`${backend}/user/${username}`);
			const resultJSON = await result.json();
			if (!result.ok)
				throw new Error(resultJSON.detail, { cause: resultJSON.errors });

			setUserData(resultJSON);
			if (currentUser && currentUser.username === resultJSON.username)
				try
				{
					const savedPostsResult = await fetch(`${backend}/user/saved-posts`, {
						headers: {
							Authorization: `Bearer ${localStorage.getItem("token")}`
						}
					});
					if (savedPostsResult.status === 401)
						return logOut(dispatch, history);
					
					const savedPostsResultJSON = await savedPostsResult.json();
					if (!savedPostsResult.ok)
						throw new Error(savedPostsResultJSON.detail, savedPostsResultJSON.errors);
					
					setSavedPosts(savedPostsResultJSON);
				}
				catch
				{
					setSavedPosts(null);
				}
		}
		catch (err)
		{
			return setUserData(false);
		}
	};

	useEffect(() =>
	{
		closeFollowListWindow();

		const getData = async () =>
		{
			dispatch(startLoading());
			await getUserData();
			dispatch(stopLoading());
		};
		getData();
	// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [username]);
	
	useEffect(() =>
	{
		if (!userData) return;
		userData.realName
			? document.title = `${userData.realName} (@${userData.username}) • Instadicey`
			: document.title = `@${userData.username} • Instadicey`;
	}, [userData]);

	if (userData === false) return <BrokenPage/>;
	else if (!userData) return null;
	else return(
		<div className="user-profile">
			{ isFollowingListWindowOpen
				? <FollowWindow title="Following" users={userData.following} closeFollowListWindow={closeFollowListWindow}/>
				: isFollowersListWindowOpen
					? <FollowWindow title="Followers" users={userData.followers} closeFollowListWindow={closeFollowListWindow}/>
					: null
			}
			<div className="upper">
				<div className="personal-info">
					<div className="profile-pic"><img src={userData.profilePic} alt={`${username}'s profile pic`}></img></div>
					<div className="info">
						<div className="name">
							{userData.username}
							<VerifiedTick user={userData}/>
							{ currentUser
								? userData.username === currentUser.username
									? <Link to="/accounts/edit" className="edit-profile-btn outlined">Edit Profile</Link>
									: <FollowButton target={userData}/>
								: <FollowButton target={userData}/>
							}
						</div>
						<div className="follow">
							<span className="posts"><span className="number">{userData.posts.length}</span> posts</span>
							<span className="followers" onClick={() => setIsFollowersListWindowOpen(true)}><span className="number">{userData.followers.length}</span> followers</span>
							<span className="following" onClick={() => setIsFollowingListWindowOpen(true)}><span className="number">{userData.following.length}</span> following</span>
						</div>
						<div className="bio">
							<span className="real-name">{userData.realName}</span>
							<div className="bio-text">{userData.bio}</div>
						</div>
					</div>
				</div>
			</div>
			<nav>
				<ul>
					<li><NavLink exact to={`/${userData.username}`} activeClassName="selected"><svg xmlns="http://www.w3.org/2000/svg" className="icon" viewBox="0 0 24 24"><path d="M6 6h-6v-6h6v6zm9-6h-6v6h6v-6zm9 0h-6v6h6v-6zm-18 9h-6v6h6v-6zm9 0h-6v6h6v-6zm9 0h-6v6h6v-6zm-18 9h-6v6h6v-6zm9 0h-6v6h6v-6zm9 0h-6v6h6v-6z"/></svg> POSTS</NavLink></li>
					{currentUser && currentUser.username === userData.username && <li><NavLink exact to={`/${userData.username}/saved`} activeClassName="selected"><svg xmlns="http://www.w3.org/2000/svg" className="icon" viewBox="0 0 24 24"><path d="M16 2v17.582l-4-3.512-4 3.512v-17.582h8zm2-2h-12v24l6-5.269 6 5.269v-24z"/></svg> SAVED</NavLink></li>}
				</ul>
			</nav>
			<Switch>
				<Route exact path="/:username">
					<div className="post-cards-container">
						{
							userData.posts.map(post =>
								<PostCard post={post} key={post.id}/>)
						}
					</div>
				</Route>
				<Route exact path="/:username/saved">
					<div className="post-cards-container">
						{ savedPosts &&
							savedPosts.map(post =>
								<PostCard post={post} key={post.id}/>)
						}
					</div>
				</Route>
			</Switch>
		</div>
	);
};

export default UserProfile;
