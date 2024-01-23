using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using XDRPCPlusPlus;
using XDevkit;

using Fody;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace LordVirusPersonalMw2RTMToolXbox;

[ConfigureAwait(false)]
public sealed partial class MainWindow
{
    internal sealed class G_ClientComboBoxItem : ComboBoxItem
    {
        public G_Client? Client;
    }

    public IXboxManager? XboxManager = null;
    public IXboxConsole? DevKit = null;

    private Random _random = new Random();

    private CancellationTokenSource? NameChangerCancellationTokenSource = null;
    private CancellationTokenSource? LevelCancellationTokenSource = null;
    private CancellationTokenSource? PrestigeCancellationTokenSource = null;

    private G_Client?[] CurrentGameClients = new G_Client?[_maxClientCount]; 

    private bool InGame
    {
        get
        {
            if (DevKit is null)
                return false;

            return Mw2GameFunctions.Cg_DvarGetBool(DevKit!, "cl_ingame");
        }
    }

    private void Internal_Connect()
    {
        try
        {
            XboxManager = new XboxManager();
            DevKit = XboxManager.OpenConsole(XboxManager.DefaultConsole);

            LaserCheckBox.IsEnabled = true;
            RedBoxCheckBox.IsEnabled = true;
            ThermalCheckBox.IsEnabled = true;
            NoRecoilCheckBox.IsEnabled = true;
            ProModCheckBox.IsEnabled = true;
            CartoonCheckBox.IsEnabled = true;
            ChromeCheckBox.IsEnabled = true;
            UiDebugCheckBox.IsEnabled = true;
            FxCheckBox.IsEnabled = true;

            NameChangerTextBox.IsEnabled = true;
            ChangeNameButton.IsEnabled = true;
            RealTimeNameChangeCheckBox.IsEnabled = true;

            ClanNameChangerTextBox.IsEnabled = true;
            ChangeClanNameButton.IsEnabled = true;

            PrestigeIntegerUpDown.Value = 10;
            PrestigeIntegerUpDown.Maximum = _maxPrestige;
            PrestigeIntegerUpDown.Minimum = _minPrestige;
            PrestigeIntegerUpDown.IsEnabled = true;
            ChangePrestigeButton.IsEnabled = true;
            LoopPrestigeCheckBox.IsEnabled = true;

            LevelIntegerUpDown.Value = _maxLevel;
            LevelIntegerUpDown.Maximum = _maxLevel;
            LevelIntegerUpDown.Minimum = _minLevel;
            LevelIntegerUpDown.IsEnabled = true;
            ChangeLevelButton.IsEnabled = true;
            LoopLevelCheckBox.IsEnabled = true;

            CBuffAddTextBox.IsEnabled = true;
            CBuffAddTextButton.IsEnabled = true;

            SendGameServerCommandTextBox.IsEnabled = true;
            SendGameServerCommandButton.IsEnabled = true;

            UnlockAllButton.IsEnabled = true;
            EndGameButton.IsEnabled = true;

            ClientComboBox.IsEnabled = true;

            Internal_UpdateCurrentUserInfo();
            Mw2GameFunctions.Cg_DvarGetBool(DevKit!, "loc_warningsUI 0; loc_warnings 0; cg_blood 0; cg_bloodLimit 0;");

            MessageBox.Show
            (
                "Successfuly Connected!",
                "Connection",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        catch (Exception ex)
        {
            NameChangerCancellationTokenSource?.Cancel();
            NameChangerCancellationTokenSource = null;

            PrestigeCancellationTokenSource?.Cancel();
            PrestigeCancellationTokenSource = null;

            LevelCancellationTokenSource?.Cancel();
            LevelCancellationTokenSource = null;

            MessageBox.Show
            (
                ex.Message,
                "Connect Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    // TODO: Get this working!
    //private bool Internal_IsLocalClientInGame(int client = 0)
    //{
    //    return DevKit?.ExecuteRPC<bool>
    //    (
    //        new XDRPCExecutionOptions(XDRPCMode.Title, 0x82182990),
    //        new object[] { client }
    //    ) ?? false;
    //}

    private void Internal_UpdateCurrentUserInfo()
    {
        Span<byte> initNameByteSpan = DevKit?.ReadBytes(_nameAddress, _maxNameInputLength);
        initNameByteSpan = initNameByteSpan.TrimEnd((byte)0x00);

        Span<char> nameChars = stackalloc char[initNameByteSpan.Length];

        Encoding.ASCII.GetChars(initNameByteSpan, nameChars);

        var stringBuilder = new StringBuilder(nameChars.Length);

        bool setColorCodeNextLoop = false;
        foreach (char c in nameChars)
        {
            if (c == '^')
            {
                stringBuilder.Append(c);
                setColorCodeNextLoop = true;
                continue;
            }

            if (setColorCodeNextLoop && c.IsInt())
            {
                stringBuilder.Append('F');
                setColorCodeNextLoop = false;
                continue;
            }

            if (_buttonCharMap.Contains(c))
            {
                stringBuilder.Append("^B");
                continue;
            }

            stringBuilder.Append(c);
        }

        NameChangerTextBox.Text = stringBuilder.ToString();

        Span<byte> initClanByteSpan = DevKit?.ReadBytes(_clanAddress, 4);
        initClanByteSpan = initClanByteSpan.TrimEnd((byte)0x00);
        ClanNameChangerTextBox.Text = Encoding.ASCII.GetString(initClanByteSpan);
    }

    private void Internal_RefreshClients()
    {
        if (DevKit is null)
            return;

        for (int clientIndex = 0; clientIndex < _maxClientCount; ++clientIndex) 
        {
            if (CurrentGameClients[clientIndex] is null)
            {
                CurrentGameClients[clientIndex] = new G_Client(DevKit!, clientIndex);

                ClientComboBox.Items.Add(new G_ClientComboBoxItem()
                {
                    Content = CurrentGameClients[clientIndex]?.ClientName,
                    Client = CurrentGameClients[clientIndex]
                });

                continue;
            }

            var item = (G_ClientComboBoxItem)ClientComboBox.Items[clientIndex];
            item.Content = item?.Client?.ClientName ?? string.Empty;
        }
    }

    private async Task Internal_UnlockAll(int client = -1)
    {
        if (DevKit is null)
            return;

        if (!InGame)
        {
            MessageBox.Show("You must be in game to unlock all!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "s loc_warningsUI 0; loc_warnings 0;");

        Mw2GameFunctions.iPrintLnBold(DevKit!, "^2Starting Challenges!", client);

        await Task.Run(async () =>
        {
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 2056 206426 6525 7F 3500 99 3501 99 3502 99 3503 99 3504 99 3505 99 3506 99 3507 99 3508 99 3509 99 3510 99 3511 99 3512 99 3513 99 3514 99 3515 99 3516 99 3517 99 3518 99 3519 99 3520 99 3521 99 3522 99 3523 99 3524 99 3525 99 3526 99 3527 99 3528 99 3529 99 3530 99 3531 99 3532 99 3533 99 3534 99 3535 99 3536 99 3537 99 3538 99 3539 99 3540 99 3541 99 3542 99 3543 99 3544 99 3545 99 3546 99 3547 99 3548 99 3549 99 3550 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3550 99 3551 99 3552 99 3553 99 3554 99 3555 99 3556 99 3557 99 3558 99 3559 99 3560 99 3561 99 3562 99 3563 99 3564 99 3565 99 3566 99 3567 99 3568 99 3569 99 3570 99 3571 99 3572 99 3573 99 3574 99 3575 99 3576 99 3577 99 3578 99 3579 99 3580 99 3581 99 3582 99 3583 99 3584 99 3585 99 3586 99 3587 99 3588 99 3589 99 3590 99 3591 99 3592 99 3593 99 3594 99 3595 99 3596 99 3597 99 3598 99 3599 99 3600 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3600 99 3601 99 3602 99 3603 99 3604 99 3605 99 3606 99 3607 99 3608 99 3609 99 3610 99 3611 99 3612 99 3613 99 3614 99 3615 99 3616 99 3617 99 3618 99 3619 99 3620 99 3621 99 3622 99 3623 99 3624 99 3625 99 3626 99 3627 99 3628 99 3629 99 3630 99 3631 99 3632 99 3633 99 3634 99 3635 99 3636 99 3637 99 3638 99 3639 99 3640 99 3641 99 3642 99 3643 99 3644 99 3645 99 3646 99 3647 99 3648 99 3649 99 3650 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3650 99 3651 99 3652 99 3653 99 3654 99 3655 99 3656 99 3657 99 3658 99 3659 99 3660 99 3661 99 3662 99 3663 99 3664 99 3665 99 3666 99 3667 99 3668 99 3669 99 3670 99 3671 99 3672 99 3673 99 3674 99 3675 99 3676 99 3677 99 3678 99 3679 99 3680 99 3681 99 3682 99 3683 99 3684 99 3685 99 3686 99 3687 99 3688 99 3689 99 3690 99 3691 99 3692 99 3693 99 3694 99 3695 99 3696 99 3697 99 3698 99 3699 99 3700 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3700 99 3701 99 3702 99 3703 99 3704 99 3705 99 3706 99 3707 99 3708 99 3709 99 3710 99 3711 99 3712 99 3713 99 3714 99 3715 99 3716 99 3717 99 3718 99 3719 99 3720 99 3721 99 3722 99 3723 99 3724 99 3725 99 3726 99 3727 99 3728 99 3729 99 3730 99 3731 99 3732 99 3733 99 3734 99 3735 99 3736 99 3737 99 3738 99 3739 99 3740 99 3741 99 3742 99 3743 99 3744 99 3745 99 3746 99 3747 99 3748 99 3749 99 3750 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3750 99 3751 99 3752 99 3753 99 3754 99 3755 99 3756 99 3757 99 3758 99 3759 99 3760 99 3761 99 3762 99 3763 99 3764 99 3765 99 3766 99 3767 99 3768 99 3769 99 3770 99 3771 99 3772 99 3773 99 3774 99 3775 99 3776 99 3777 99 3778 99 3779 99 3780 99 3781 99 3782 99 3783 99 3784 99 3785 99 3786 99 3787 99 3788 99 3789 99 3790 99 3791 99 3792 99 3793 99 3794 99 3795 99 3796 99 3797 99 3798 99 3799 99 3800 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3800 99 3801 99 3802 99 3803 99 3804 99 3805 99 3806 99 3807 99 3808 99 3809 99 3810 99 3811 99 3812 99 3813 99 3814 99 3815 99 3816 99 3817 99 3818 99 3819 99 3820 99 3821 99 3822 99 3823 99 3824 99 3825 99 3826 99 3827 99 3828 99 3829 99 3830 99 3831 99 3832 99 3833 99 3834 99 3835 99 3836 99 3837 99 3838 99 3839 99 3840 99 3841 99 3842 99 3843 99 3844 99 3845 99 3846 99 3847 99 3848 99 3849 99 3850 99");
            Mw2GameFunctions.iPrintLn(DevKit!, "25 ^9Percent ^4Unlocked", client);
            await Task.Delay(TimeSpan.FromSeconds(4));
        });

        await Task.Run(async () =>
        {
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3850 99 3851 99 3852 99 3853 99 3854 99 3855 99 3856 99 3857 99 3858 99 3859 99 3860 99 3861 99 3862 99 3863 99 3864 99 3865 99 3866 99 3867 99 3868 99 3869 99 3870 99 3871 99 3872 99 3873 99 3874 99 3875 99 3876 99 3877 99 3878 99 3879 99 3880 99 3881 99 3882 99 3883 99 3884 99 3885 99 3886 99 3887 99 3888 99 3889 99 3890 99 3891 99 3892 99 3893 99 3894 99 3895 99 3896 99 3897 99 3898 99 3899 99 3900 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3900 99 3901 99 3902 99 3903 99 3904 99 3905 99 3906 99 3907 99 3908 99 3909 99 3910 99 3911 99 3912 99 3913 99 3914 99 3915 99 3916 99 3917 99 3918 99 3919 99 3920 99 3921 99 3922 99 3923 99 3924 99 3925 99 3926 99 3927 99 3928 99 3929 99 3930 99 3931 99 3932 99 3933 99 3934 99 3935 99 3936 99 3937 99 3938 99 3939 99 3940 99 3941 99 3942 99 3943 99 3944 99 3945 99 3946 99 3947 99 3948 99 3949 99 3950 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 3950 99 3951 99 3952 99 3953 99 3954 99 3955 99 3956 99 3957 99 3958 99 3959 99 3960 99 3961 99 3962 99 3963 99 3964 99 3965 99 3966 99 3967 99 3968 99 3969 99 3970 99 3971 99 3972 99 3973 99 3974 99 3975 99 3976 99 3977 99 3978 99 3979 99 3980 99 3981 99 3982 99 3983 99 3984 99 3985 99 3986 99 3987 99 3988 99 3989 99 3990 99 3991 99 3992 99 3993 99 3994 99 3995 99 3996 99 3997 99 3998 99 3999 99 4000 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4000 99 4001 99 4002 99 4003 99 4004 99 4005 99 4006 99 4007 99 4008 99 4009 99 4010 99 4011 99 4012 99 4013 99 4014 99 4015 99 4016 99 4017 99 4018 99 4019 99 4020 99 4021 99 4022 99 4023 99 4024 99 4025 99 4026 99 4027 99 4028 99 4029 99 4030 99 4031 99 4032 99 4033 99 4034 99 4035 99 4036 99 4037 99 4038 99 4039 99 4040 99 4041 99 4042 99 4043 99 4044 99 4045 99 4046 99 4047 99 4048 99 4049 99 4050 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4050 99 4051 99 4052 99 4053 99 4054 99 4055 99 4056 99 4057 99 4058 99 4059 99 4060 99 4061 99 4062 99 4063 99 4064 99 4065 99 4066 99 4067 99 4068 99 4069 99 4070 99 4071 99 4072 99 4073 99 4074 99 4075 99 4076 99 4077 99 4078 99 4079 99 4080 99 4081 99 4082 99 4083 99 4084 99 4085 99 4086 99 4087 99 4088 99 4089 99 4090 99 4091 99 4092 99 4093 99 4094 99 4095 99 4096 99 4097 99 4098 99 4099 99 4100 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4100 99 4101 99 4102 99 4103 99 4104 99 4105 99 4106 99 4107 99 4108 99 4109 99 4110 99 4111 99 4112 99 4113 99 4114 99 4115 99 4116 99 4117 99 4118 99 4119 99 4120 99 4121 99 4122 99 4123 99 4124 99 4125 99 4126 99 4127 99 4128 99 4129 99 4130 99 4131 99 4132 99 4133 99 4134 99 4135 99 4136 99 4137 99 4138 99 4139 99 4140 99 4141 99 4142 99 4143 99 4144 99 4145 99 4146 99 4147 99 4148 99 4149 99 4150 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4150 99 4151 99 4152 99 4153 99 4154 99 4155 99 4156 99 4157 99 4158 99 4159 99 4160 99 4161 99 4162 99 4163 99 4164 99 4165 99 4166 99 4167 99 4168 99 4169 99 4170 99 4171 99 4172 99 4173 99 4174 99 4175 99 4176 99 4177 99 4178 99 4179 99 4180 99 4181 99 4182 99 4183 99 4184 99 4185 99 4186 99 4187 99 4188 99 4189 99 4190 99 4191 99 4192 99 4193 99 4194 99 4195 99 4196 99 4197 99 4198 99 4199 99 4200 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4200 99 4201 99 4202 99 4203 99 4204 99 4205 99 4206 99 4207 99 4208 99 4209 99 4210 99 4211 99 4212 99 4213 99 4214 99 4215 99 4216 99 4217 99 4218 99 4219 99 4220 99 4221 99 4222 99 4223 99 4224 99 4225 99 4226 99 4227 99 4228 99 4229 99 4230 99 4231 99 4232 99 4233 99 4234 99 4235 99 4236 99 4237 99 4238 99 4239 99 4240 99 4241 99 4242 99 4243 99 4244 99 4245 99 4246 99 4247 99 4248 99 4249 99 4250 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4250 99 4251 99 4252 99 4253 99 4254 99 4255 99 4256 99 4257 99 4258 99 4259 99 4260 99 4261 99 4262 99 4263 99 4264 99 4265 99 4266 99 4267 99 4268 99 4269 99 4270 99 4271 99 4272 99 4273 99 4274 99 4275 99 4276 99 4277 99 4278 99 4279 99 4280 99 4281 99 4282 99 4283 99 4284 99 4285 99 4286 99 4287 99 4288 99 4289 99 4290 99 4291 99 4292 99 4293 99 4294 99 4295 99 4296 99 4297 99 4298 99 4299 99 4300 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4300 99 4301 99 4302 99 4303 99 4304 99 4305 99 4306 99 4307 99 4308 99 4309 99 4310 99 4311 99 4312 99 4313 99 4314 99 4315 99 4316 99 4317 99 4318 99 4319 99 4320 99 4321 99 4322 99 4323 99 4324 99 4325 99 4326 99 4327 99 4328 99 4329 99 4330 99 4331 99 4332 99 4333 99 4334 99 4335 99 4336 99 4337 99 4338 99 4339 99 4340 99 4341 99 4342 99 4343 99 4344 99 4345 99 4346 99 4347 99 4348 99 4349 99 4350 99");
            Mw2GameFunctions.iPrintLn(DevKit!, "50 ^9Percent ^4Unlocked", client);
            await Task.Delay(TimeSpan.FromSeconds(4));
        });

        await Task.Run(async () =>
        {
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4350 99 4351 99 4352 99 4353 99 4354 99 4355 99 4356 99 4357 99 4358 99 4359 99 4360 99 4361 99 4362 99 4363 99 4364 99 4365 99 4366 99 4367 99 4368 99 4369 99 4370 99 4371 99 4372 99 4373 99 4374 99 4375 99 4376 99 4377 99 4378 99 4379 99 4380 99 4381 99 4382 99 4383 99 4384 99 4385 99 4386 99 4387 99 4388 99 4389 99 4390 99 4391 99 4392 99 4393 99 4394 99 4395 99 4396 99 4397 99 4398 99 4399 99 4400 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4400 99 4401 99 4402 99 4403 99 4404 99 4405 99 4406 99 4407 99 4408 99 4409 99 4410 99 4411 99 4412 99 4413 99 4414 99 4415 99 4416 99 4417 99 4418 99 4419 99 4420 99 4421 99 4422 99 4423 99 4424 99 4425 99 4426 99 4427 99 4428 99 4429 99 4430 99 4431 99 4432 99 4433 99 4434 99 4435 99 4436 99 4437 99 4438 99 4439 99 4440 99 4441 99 4442 99 4443 99 4444 99 4445 99 4446 99 4447 99 4448 99 4449 99 4450 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4450 99 4451 99 4452 99 4453 99 4454 99 4455 99 4456 99 4457 99 4458 99 4459 99 4460 99 4461 99 4462 99 4463 99 4464 99 4465 99 4466 99 4467 99 4468 99 4469 99 4470 99 4471 99 4472 99 4473 99 4474 99 4475 99 4476 99 4477 99 4478 99 4479 99 4480 99 4481 99 4482 99 4483 99 4484 99 4485 99 4486 99 4487 99 4488 99 4489 99 4490 99 4491 99 4492 99 4493 99 4494 99 4495 99 4496 99 4497 99 4498 99 4499 99 4500 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4500 99 4501 99 4502 99 4503 99 4504 99 4505 99 4506 99 4507 99 4508 99 4509 99 4510 99 4511 99 4512 99 4513 99 4514 99 4515 99 4516 99 4517 99 4518 99 4519 99 4520 99 4521 99 4522 99 4523 99 4524 99 4525 99 4526 99 4527 99 4528 99 4529 99 4530 99 4531 99 4532 99 4533 99 4534 99 4535 99 4536 99 4537 99 4538 99 4539 99 4540 99 4541 99 4542 99 4543 99 4544 99 4545 99 4546 99 4547 99 4548 99 4549 99 4550 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4550 99 4551 99 4552 99 4553 99 4554 99 4555 99 4556 99 4557 99 4558 99 4559 99 4560 99 4561 99 4562 99 4563 99 4564 99 4565 99 4566 99 4567 99 4568 99 4569 99 4570 99 4571 99 4572 99 4573 99 4574 99 4575 99 4576 99 4577 99 4578 99 4579 99 4580 99 4581 99 4582 99 4583 99 4584 99 4585 99 4586 99 4587 99 4588 99 4589 99 4590 99 4591 99 4592 99 4593 99 4594 99 4595 99 4596 99 4597 99 4598 99 4599 99 4600 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4600 99 4601 99 4602 99 4603 99 4604 99 4605 99 4606 99 4607 99 4608 99 4609 99 4610 99 4611 99 4612 99 4613 99 4614 99 4615 99 4616 99 4617 99 4618 99 4619 99 4620 99 4621 99 4622 99 4623 99 4624 99 4625 99 4626 99 4627 99 4628 99 4629 99 4630 99 4631 99 4632 99 4633 99 4634 99 4635 99 4636 99 4637 99 4638 99 4639 99 4640 99 4641 99 4642 99 4643 99 4644 99 4645 99 4646 99 4647 99 4648 99 4649 99 4650 99");
            Mw2GameFunctions.iPrintLn(DevKit!, "75 ^9Percent ^4Unlocked", client);
            await Task.Delay(TimeSpan.FromSeconds(4));
        });

        await Task.Run(async () =>
        {
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4650 99 4651 99 4652 99 4653 99 4654 99 4655 99 4656 99 4657 99 4658 99 4659 99 4660 99 4661 99 4662 99 4663 99 4664 99 4665 99 4666 99 4667 99 4668 99 4669 99 4670 99 4671 99 4672 99 4673 99 4674 99 4675 99 4676 99 4677 99 4678 99 4679 99 4680 99 4681 99 4682 99 4683 99 4684 99 4685 99 4686 99 4687 99 4688 99 4689 99 4690 99 4691 99 4692 99 4693 99 4694 99 4695 99 4696 99 4697 99 4698 99 4699 99 4700 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4700 99 4701 99 4702 99 4703 99 4704 99 4705 99 4706 99 4707 99 4708 99 4709 99 4710 99 4711 99 4712 99 4713 99 4714 99 4715 99 4716 99 4717 99 4718 99 4719 99 4720 99 4721 99 4722 99 4723 99 4724 99 4725 99 4726 99 4727 99 4728 99 4729 99 4730 99 4731 99 4732 99 4733 99 4734 99 4735 99 4736 99 4737 99 4738 99 4739 99 4740 99 4741 99 4742 99 4743 99 4744 99 4745 99 4746 99 4747 99 4748 99 4749 99 4750 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4750 99 4751 99 4752 99 4753 99 4754 99 4755 99 4756 99 4757 99 4758 99 4759 99 4760 99 4761 99 4762 99 4763 99 4764 99 4765 99 4766 99 4767 99 4768 99 4769 99 4770 99 4771 99 4772 99 4773 99 4774 99 4775 99 4776 99 4777 99 4778 99 4779 99 4780 99 4781 99 4782 99 4783 99 4784 99 4785 99 4786 99 4787 99 4788 99 4789 99 4790 99 4791 99 4792 99 4793 99 4794 99 4795 99 4796 99 4797 99 4798 99 4799 99 4800 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4800 99 4801 99 4802 99 4803 99 4804 99 4805 99 4806 99 4807 99 4808 99 4809 99 4810 99 4811 99 4812 99 4813 99 4814 99 4815 99 4816 99 4817 99 4818 99 4819 99 4820 99 4821 99 4822 99 4823 99 4824 99 4825 99 4826 99 4827 99 4828 99 4829 99 4830 99 4831 99 4832 99 4833 99 4834 99 4835 99 4836 99 4837 99 4838 99 4839 99 4840 99 4841 99 4842 99 4843 99 4844 99 4845 99 4846 99 4847 99 4848 99 4849 99 4850 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4850 99 4851 99 4852 99 4853 99 4854 99 4855 99 4856 99 4857 99 4858 99 4859 99 4860 99 4861 99 4862 99 4863 99 4864 99 4865 99 4866 99 4867 99 4868 99 4869 99 4870 99 4871 99 4872 99 4873 99 4874 99 4875 99 4876 99 4877 99 4878 99 4879 99 4880 99 4881 99 4882 99 4883 99 4884 99 4885 99 4886 99 4887 99 4888 99 4889 99 4890 99 4891 99 4892 99 4893 99 4894 99 4895 99 4896 99 4897 99 4898 99 4899 99 4900 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4900 99 4901 99 4902 99 4903 99 4904 99 4905 99 4906 99 4907 99 4908 99 4909 99 4910 99 4911 99 4912 99 4913 99 4914 99 4915 99 4916 99 4917 99 4918 99 4919 99 4920 99 4921 99 4922 99 4923 99 4924 99 4925 99 4926 99 4927 99 4928 99 4929 99 4930 99 4931 99 4932 99 4933 99 4934 99 4935 99 4936 99 4937 99 4938 99 4939 99 4940 99 4941 99 4942 99 4943 99 4944 99 4945 99 4946 99 4947 99 4948 99 4949 99 4950 99");
            Mw2GameFunctions.Cg_GameSendServerCommand(DevKit!, client, 0, "J 4950 99 4951 99 4952 99 4953 99 4954 99 4955 99 4956 99 4957 99 4958 99 4959 99 4960 99 4961 99 4962 99 4963 99 4964 99 4965 99 4966 99 4967 99 4968 99 4969 99 4970 99 4971 99 4972 99 4973 99 4974 99 4975 99 4976 99 4977 99 4978 99 4979 99 4980 99 4981 99 4982 99 4983 99 4984 99 4985 99 4986 99 4987 99 4988 99 4989 99 4990 99 4991 99 4992 99 4993 99 4994 99 4995 99 4996 99 4997 99 4998 99 4999 99 5000 99");
            Mw2GameFunctions.iPrintLn(DevKit!, "100 ^9Percent ^4Unlocked", client);
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        });

        Mw2GameFunctions.iPrintLnBold(DevKit!, "^2Completed Challenges!", client);
    }

    private void Internal_SetPrestige(int prestige = 10)
    {
        if (prestige < _minLevel)
            return;

        if (prestige > _maxPrestige)
            return;

        byte[] prestigeBytes = BitConverter
            .GetBytes(prestige)
            .ToArray();

        DevKit?.DebugTarget
            .SetMemory
            (
                _prestigeAddress, 
                (uint)prestigeBytes.Length, 
                prestigeBytes, 
                out _
            );
    }

    private void Internal_SetLevel(int level = 70)
    {
        if (level < _minLevel)
            return;

        if (level > _maxLevel)
            return;

        byte[] levelBytes = BitConverter
            .GetBytes(_levelTable[level - 1])
            .ToArray();

        DevKit?.DebugTarget
            .SetMemory
            (
                _levelAddress, 
                (uint)levelBytes.Length,
                levelBytes,
                out _
            );
    }

    private void Internal_SetName(ReadOnlySpan<char> newName)
    {
        if (newName.Length > _maxNameInputLength)
            newName = newName[.._maxNameInputLength];

        Span<byte> nameBytes = stackalloc byte[_maxNameInputLength];
        Encoding.ASCII.GetBytes(newName, nameBytes);

        //if (InGame)
        //{
        //    Internal_CbufAddText($"userinfo \"\\clanabbrev\\^{_random.Next(6)}\\name\\{newName}\"");

        //    return;
        //}

        DevKit?.DebugTarget
            .SetMemory
            (
                _nameAddress,
                (uint)nameBytes.Length,
                nameBytes.ToArray(),
                out _
            );
    }

    private void Internal_SetClanName(ReadOnlySpan<char> newClanName)
    {
        if (newClanName.Length > 4)
            newClanName = newClanName[..4];

        Span<byte> clanNameBytes = stackalloc byte[4];
        Encoding.ASCII.GetBytes(newClanName, clanNameBytes);

        DevKit?.DebugTarget
            .SetMemory
            (
                _clanAddress, 
                (uint)clanNameBytes.Length,
                clanNameBytes.ToArray(),
                out _
            );
    }

    private void Internal_SetRealTimeNameChanging(bool toggleValue)
    {
        if(toggleValue)
        {
            NameChangerCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_AutoUpdateName(NameChangerCancellationTokenSource.Token);

            ChangeNameButton.IsEnabled = false;
            RainbowCheckBox.IsEnabled = true;
            ButtonCheckBox.IsEnabled = true;

            return;
        }

        if (NameChangerCancellationTokenSource is null)
            return;

        NameChangerCancellationTokenSource.Cancel();
        NameChangerCancellationTokenSource = null;

        ChangeNameButton.IsEnabled = true;

        RainbowCheckBox.IsEnabled = false;
        RainbowCheckBox.IsChecked = false;

        ButtonCheckBox.IsEnabled = false;
        ButtonCheckBox.IsChecked = false;

        NameChangerTextBox.MaxLength = _maxNameInputLength;
    }

    private void Internal_SetLevelLooping(bool toggleValue)
    {
        if (toggleValue)
        {
            LevelCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_LoopLevels(LevelCancellationTokenSource.Token);

            LevelIntegerUpDown.IsEnabled = false;
            ChangeLevelButton.IsEnabled = false;

            return;
        }

        if (LevelCancellationTokenSource is null)
            return;

        LevelCancellationTokenSource.Cancel();
        LevelCancellationTokenSource = null;

        LevelIntegerUpDown.IsEnabled = true;
        ChangeLevelButton.IsEnabled = true;
    }

    private void Internal_SetPrestigeLooping(bool toggleValue)
    {
        if (toggleValue)
        {
            PrestigeCancellationTokenSource = new CancellationTokenSource();
            _ = Internal_LoopPrestiges(PrestigeCancellationTokenSource.Token);

            PrestigeIntegerUpDown.IsEnabled = false;
            ChangePrestigeButton.IsEnabled = false;

            return;
        }

        if (PrestigeCancellationTokenSource is null)
            return;

        PrestigeCancellationTokenSource.Cancel();
        PrestigeCancellationTokenSource = null;

        PrestigeIntegerUpDown.IsEnabled = true;
        ChangePrestigeButton.IsEnabled = true;
    }

    private ReadOnlySpan<char> Internal_BuildAutoUpdatingNameString()
    {
        byte tempMaxNameInputLength = _maxNameInputLength;

        ReadOnlySpan<char> newNameBuffer = NameChangerTextBox.Text;

        if (newNameBuffer.Length < _maxNameInputLength)
            goto ParseSpan;

        for (int i = 0; i < newNameBuffer.Length; ++i)
        {
            ++i;
            if (!(newNameBuffer[i - 1] == '^' &&
                (newNameBuffer[i] == 'B' ||
                    newNameBuffer[i] == 'b')))
                continue;

            tempMaxNameInputLength++;
        }

        // Slice to buffer length;
        newNameBuffer = newNameBuffer[..tempMaxNameInputLength];

        ParseSpan:
        if (RainbowCheckBox.IsChecked ?? false) 
            Internal_ParseFlashingCodes(newNameBuffer, out newNameBuffer);

        if (ButtonCheckBox.IsChecked ?? false)
            Internal_ParseButtonCodes(newNameBuffer, out newNameBuffer);

        return newNameBuffer;
    }

    private void Internal_ParseFlashingCodes(ReadOnlySpan<char> name, out ReadOnlySpan<char> newName)
    {
        Span<char> tempName = stackalloc char[name.Length];
        name.CopyTo(tempName);

        int flashIndex = name.IndexOf("^F", StringComparison.OrdinalIgnoreCase);
        while (flashIndex > -1)
        {
            tempName[flashIndex] = '^'; flashIndex++;
            tempName[flashIndex] = (char)(_random.Next(6) + 48); flashIndex++;

            var newflashIndex = name[flashIndex..].IndexOf("^F", StringComparison.OrdinalIgnoreCase);

            if (newflashIndex == -1)
                break;

            flashIndex = newflashIndex + flashIndex;
        }

        newName = tempName.ToArray();
    }

    private void Internal_ParseButtonCodes(ReadOnlySpan<char> inputName, out ReadOnlySpan<char> newName)
    {
        Span<char> tempName = stackalloc char[inputName.Length];
        inputName.CopyTo(tempName);

        int buttonIndex = inputName.IndexOf("^B", StringComparison.OrdinalIgnoreCase);
        int removalCount = 0;
        while (buttonIndex > -1)
        {
            // Generate random button char.
            tempName[buttonIndex - removalCount] = _buttonCharMap[_random.Next(_buttonCharMap.Length)]; 

            Span<char> tempNameSlice = tempName[(buttonIndex - removalCount + 2)..].ToArray();
            tempNameSlice.TryCopyTo(tempName[(buttonIndex - removalCount + 1)..]);

            removalCount++;

            if (inputName.Length < buttonIndex + 2)
                break;

            int tempIndex = buttonIndex + 2;
            buttonIndex = inputName[tempIndex..].IndexOf("^B", StringComparison.OrdinalIgnoreCase);

            if (buttonIndex == -1)
                break;

            buttonIndex += tempIndex;
        }

        newName = tempName[..(tempName.Length - removalCount)].ToArray();
    }

    private async Task Internal_AutoUpdateName(CancellationToken cancellationToken)
    {
        do
        {
            Internal_SetName(Internal_BuildAutoUpdatingNameString());

            TimeSpan delay = TimeSpan.FromMicroseconds(150);

            await Task
                .Delay(delay, cancellationToken)
                .ConfigureAwait(true);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task Internal_LoopLevels(CancellationToken cancellationToken)
    {
        int level = 1;

        do
        {
            Internal_SetLevel(level++);

            level = level % (_maxLevel + 1);

            await Task.Delay(TimeSpan.FromMilliseconds(70), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }

    private async Task Internal_LoopPrestiges(CancellationToken cancellationToken)
    {
        int prestige = 1;

        do
        {
            Internal_SetPrestige(prestige++);

            prestige = prestige % (_maxPrestige + 1);

            await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);
        }
        while (!cancellationToken.IsCancellationRequested);
    }
}