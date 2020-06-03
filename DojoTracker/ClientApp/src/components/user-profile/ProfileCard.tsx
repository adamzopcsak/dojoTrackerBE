import React from "react";
import { IBasicUserInfo } from "../../static/util/interfaces";
import { StyledCard } from "../styled-components/Reusables";
import styled from "styled-components";

const StyledProfileCard = styled.div`
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    font-size: 1rem;
    box-sizing: border-box;
    width: 100%;
    padding: 1rem;
    margin: 0 0 1rem 0;

    img {
        width: 6rem;
        height: auto;
        border-radius: 50%;
    }
    p {
        margin: 0.5rem;
        color: gray;
    }
    h3 {
        color: #dc3545;
    }
`;

interface Props {
    user: IBasicUserInfo;
}

const ProfileCard = (props: Props) => {
    return (
        <StyledProfileCard>
            <img src={require("../../static/img/profile-placeholder.png")}></img>
            <h3>
                {props.user.firstName} {props.user.lastName}
            </h3>
            <p>{props.user.email}</p>
        </StyledProfileCard>
    );
};

export default ProfileCard;