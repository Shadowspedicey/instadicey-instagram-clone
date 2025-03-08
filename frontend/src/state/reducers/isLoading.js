const isLoading = (state = false, action) =>
{
	switch (action.type)
	{
		case "START_LOADING":
			return state = true;

		case "STOP_LOADING":
			return state = false;
			
		default:
			return state;
	}
};

export default isLoading;
