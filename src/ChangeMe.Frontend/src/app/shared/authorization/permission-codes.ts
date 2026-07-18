export const PermissionCodes = {
  usersView: 'Users.View',
  usersManage: 'Users.Manage',
  usersDeactivate: 'Users.Deactivate',
  usersInvite: 'Users.Invite',
  rolesView: 'Roles.View',
  rolesManage: 'Roles.Manage',
  sessionsViewOwn: 'Sessions.ViewOwn',
  sessionsManageOwn: 'Sessions.ManageOwn',
  sessionsViewAny: 'Sessions.ViewAny',
  sessionsManageAny: 'Sessions.ManageAny',
  issuesView: 'Issues.View',
  issuesCreate: 'Issues.Create',
  issuesEdit: 'Issues.Edit',
  issuesDelete: 'Issues.Delete',
  issuesComment: 'Issues.Comment',
  issuesManageAttachments: 'Issues.ManageAttachments'
} as const;
