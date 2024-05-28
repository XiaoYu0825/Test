//
//  AudioGvoice.h
//  NirvanaIOS
//
//  Created by njmac on 2018/3/20.
//  Copyright © 2018年 nirvana. All rights reserved.
//
#ifndef AudioGvoice_h
#define AudioGvoice_h
#import <Foundation/Foundation.h>

@interface AudioGvoiceListener : NSObject
@property (nonatomic, copy) void(^onRecorderCallback)(NSString *fileId, NSString *str);
@property (nonatomic, copy) void(^onPlayerCallback)(void);
@property (nonatomic, copy) void(^onErrorCallback)(NSString *ret, NSString *msg);
@end

@interface AudioGvoice : NSObject

+(AudioGvoice *)getInstence;
+(void)setListener:(AudioGvoiceListener *)listener;

+(BOOL)getInitResult;
+(void)initGvoice;
+(void)startRecorder;
+(void)stopRecorder;
+(void)startPlay:(NSString *)fileID;
+(void)stopPlay;

@end
#endif /* AudioGvoice_h */
