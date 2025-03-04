import { useEffect, useState } from "react";
import { Route, Switch, useLocation } from "react-router";
import { useDispatch, useSelector } from "react-redux";
import { Snackbar, Slide, Alert } from "@mui/material";
import { stopLoading } from "./state/actions/isLoading";
import { setUser } from "./state/actions/currentUser";
import jwt from "jsonwebtoken";
import { closeSnackbar, setSnackbar } from "./state/actions/snackbar";

import Navbar from "./components/Navbar";
import LoadingPage from "./components/LoadingPage";

import SignUpPage from "./components/AccountAuth/SignUpPage";
import PasswordReset from "./components/AccountAuth/PasswordReset";
import LoginPage from "./components/AccountAuth/LoginPage";
import AccountVerification from "./components/AccountAuth/AccountVerification";
import AccountEdit from "./components/AccountEdit/AccountEdit";

import HomePage from "./components/HomePage";
import UserProfile from "./components/UserProfile";
import newPost from "./components/Posts/NewPost";
import PostPage from "./components/Posts/PostPage";

import Inbox from "./components/Inbox/Inbox";

import BrokenPage from "./components/BrokenPage";
import "./styles/App.css";
import { refreshOrLogout } from "./helpers";
import { useHistory } from "react-router-dom/cjs/react-router-dom.min";

const App = () =>
{
	const dispatch = useDispatch();
	const history = useHistory();
	const location = useLocation();
	const [navbarVisibility, setNavbarVisibility] = useState(true);
	const isLoggedIn = useSelector(state => state.currentUser);
	const isLoading = useSelector(state => state.loading);
	const snackbar = useSelector(state => state.snackbar);
	snackbar.handleClose = () => dispatch(closeSnackbar());

	useEffect(() =>
	{
		const url = location.pathname;
		if (url === "/create/style")
			setNavbarVisibility(false);
		else setNavbarVisibility(true);
	}, [location.pathname]);

	const checkIfLoggedIn = () =>
	{
		const storedToken = localStorage.getItem("token");
		if (storedToken !== null)
		{
			const claims = jwt.decode(storedToken);
			dispatch(setUser(claims));
		}
	};
	useEffect(checkIfLoggedIn, [dispatch]);
	useEffect(() =>
	{
		// 5 min interval
		if (isLoggedIn)
		{
			const refreshTokenInterval = setInterval(async () => {
				try {
					await refreshOrLogout(dispatch, history);
				} catch(err) {
					if (err.message.includes("Failed to fetch"))
						dispatch(setSnackbar("Server is down.", "error"));
				}
			}, 4*60*1000);
			return () => clearInterval(refreshTokenInterval);
		}
	}, [isLoggedIn]);

	return (
		<div className="App" style={navbarVisibility ? { paddingTop: "75px" } : null}>
			{ navbarVisibility ? <Navbar/> : null }
			{
				isLoading
					? <Route path="/" component={LoadingPage}></Route>
					: null
			}

			<Switch>
				<Route exact path="/">
					{
						isLoggedIn
							? <HomePage/>
							: <LoginPage/>
					}
				</Route>
				<Route exact path="/accounts/email-signup" component={SignUpPage}></Route>
				<Route exact path="/accounts/password/reset" component={PasswordReset}></Route>
				<Route path="/accounts/verify" component={AccountVerification}></Route>
				
				<Route exact path="/accounts/edit" component={AccountEdit}></Route>
				<Route exact path="/accounts/password/change" component={AccountEdit}></Route>

				<Route exact path="/direct/inbox" component={Inbox}></Route>
				<Route exact path="/direct/t/:roomID" component={Inbox}></Route>

				<Route exact path="/:username" component={UserProfile}></Route>
				<Route exact path="/:username/saved" component={UserProfile}></Route>
				<Route exact path="/create/style" component={newPost}></Route>

				<Route exact path="/p/:postID" component={PostPage}></Route>

				<Route path="/" component={BrokenPage}/>
			</Switch>

			<Snackbar
				open={snackbar.open}
				message={snackbar.message}
				autoHideDuration={5000}
				onClose={snackbar.handleClose}
				anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
				TransitionComponent={Slide}
				className="snackbar"
				sx={{
					"& .MuiAlert-message": {
						fontWeight: "bold"
					},
					"& .MuiAlert-action": {
						paddingLeft: "4px"
					}
				}}
			>
				<Alert onClose={snackbar.handleClose} severity={snackbar.severity} icon={false}>{snackbar.message}</Alert>
			</Snackbar>
		</div>
	);
};

export default App;
